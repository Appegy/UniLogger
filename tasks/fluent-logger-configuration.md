# Таска: Fluent `LoggerConfiguration` для инициализации ULogger

Ветка: `ivan/logger-init-task` (от `origin/main`). Type префикс коммитов — `feat`/`refactor` по конвенции репо.

## Контекст (зачем)

Сейчас инициализация — последовательность статических вызовов:

```csharp
ULogger.Initialize();
ULogger.AddTarget(new UnityTarget(formatter, filterer));
ULogger.AddTarget(new FileTarget(path, formatter: fmt));
```

Боли (подтверждены чтением кода):

1. **Окно потери логов.** `Initialize()` (`src/Runtime/ULogger.Lifecycle.cs:22-25`) сразу подменяет `Debug.unityLogger.logHandler` и вешает `Application.logMessageReceivedThreaded`, но `ULoggerData.Targets` ещё пуст. Лог между `Initialize()` и первым `AddTarget` видит `Data != null` → обходит пустые sync/async-таргеты → **молча теряется** (фоллбэк на `Debug.unityLogger` только при `Data == null`).
2. **`new Filterer(true)` — opaque** (что значит `true`?).
3. **Два способа сказать одно:** `SetAllowed(LogLevel.Trace, false)` ≡ `Disable(LogLevel.Trace)`.
4. **Boilerplate 3-в-1** на каждый таргет.
5. **Нет атомарной реконфигурации** — только `AddTarget` и `Terminate()→Initialize()`.

Цель: Serilog-style fluent `LoggerConfiguration`, который собирает конфиг целиком и применяет **атомарно** (таргеты в `ULoggerData` до активации хуков). Модель `Filterer` (bool[4] уровней + HashSet тегов + immutable snapshot) и контракт таргетов НЕ меняются — меняется только эргономика конструирования.

## Целевой публичный API

```csharp
ULogger.Configure(c => c
    .WriteTo.Unity(t => t
        .Format(FormatOptions.RichText | FormatOptions.Tags)
        .Filter(Filterer.AllowAllTags().Disable(LogLevel.Trace).Mute("Input")))
    .WriteTo.File(t => t
        .Path(path)
        .Format(FormatOptions.Time | FormatOptions.Tags | FormatOptions.LogType)
        .RollEvery(1024 * 1024)
        .Keep(5))
    .WriteTo.Memory(t => t.Capacity(64 * 1024))
    .WriteTo.Target(new MyCustomTarget()));   // escape-hatch для готовых/кастомных таргетов
```

Публичный init-API после рефактора:
- `ULogger.Configure(Action<LoggerConfiguration> configure)` — основной, атомарный (main-thread only).
- `ULogger.Initialize()` — back-compat алиас = `Configure(_ => {})` (init без таргетов, дальше runtime `AddTarget`). Поведение идентично текущему.
- `ULogger.AddTarget<T>(T)` — без изменений (runtime-добавление таргетов).
- `ULogger.Terminate()` — без изменений.

Builder-дизайн: `.Format(FormatOptions)` + `.Filter(Filterer)` + специфичные таргету ручки. Опущено `.Filter(...)` → `Filterer.AllowAllTags()` (parity с дефолтом `Target` = `new Filterer(true)`). Опущено `.Format(...)` → `FormatOptions.None`. Фильтр передаётся **экземпляром** через фабрику-цепочку на самом `Filterer` (единый source of truth, без лямбд-замыканий).

## Дизайн атомарного `Configure` (ключевой момент корректности)

Порядок операций внутри `Configure` (доказано безопасным через чтение `LogDispatcher.cs:14-23,46-86` и `ULoggerData.cs:17-31`):

1. `ThreadDispatcher.EnsureMainThread(nameof(Configure))`.
2. `var configuration = new LoggerConfiguration(); configure(configuration);`
3. `if (Data != null) Terminate();`
4. `var data = new ULoggerData();`
5. `foreach (var t in configuration.BuildTargets()) if (!data.AddTarget(t)) throw new InvalidOperationException($"Duplicate target type '{t.GetType().Name}'.");` — `ULoggerData.AddTarget` возвращает `false` на дубль типа (parity с текущим `ULogger.AddTarget`, который кидает).
6. `data.Dispatcher = new LogDispatcher(data);` — writer-тред стартует сразу, но читает `volatile`-массивы таргетов лениво; таргеты уже на месте → безопасно.
7. `OriginalHandler = Debug.unityLogger.logHandler;`
8. `ManagedStackTraceConverter.SetProjectRoot(Application.dataPath);`
9. `Data = data;` — **статический `Data` становится виден хукам только теперь, уже с таргетами.**
10. `Debug.unityLogger.logHandler = new UnityLogger();`
11. `Application.logMessageReceivedThreaded += OnNativeLogReceived;`
12. `TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;`
13. `Application.quitting += Terminate;`

Почему можно так переупорядочить: `OnNativeLogReceived` (`ULogger.cs:47`) читает статический `Data`, но вызывается только после шага 11. `LogDispatcher` хранит ссылку на `ULoggerData` (не на статический `Data`) и читает живые массивы — наполнять таргеты до/после его ctor одинаково безопасно через `volatile` swap в `ULoggerData.AddTarget`.

## Файлы

### Создать
- `src/Runtime/LoggerConfiguration.cs` — `sealed class LoggerConfiguration` (держит `List<Target>`, свойство `WriteTo`, internal `IReadOnlyList<Target> BuildTargets()`, internal `Add(Target)`) + `sealed class LoggerSinkConfiguration` (методы `Unity/File/Memory/Target`, каждый возвращает `LoggerConfiguration` для чейнинга). `WriteTo` — get-only свойство, инициализируется в ctor (`new LoggerSinkConfiguration(this)`), одна аллокация на конфиг.
- `src/Runtime/Targets/Unity/UnityTargetBuilder.cs` — `Format(FormatOptions)`, `Filter(Filterer)`, internal `Build() → new UnityTarget(new Formatter(_format), _filterer ?? Filterer.AllowAllTags())`.
- `src/Runtime/Targets/File/FileTargetBuilder.cs` — `Path(string)`, `Format`, `Filter`, `RollEvery(long)`, `Keep(int)`, `AutoFlush(bool = true)`, `Build() → new FileTarget(_path, _rollEveryBytes, _retainedFileCount, _autoFlush, formatter, filterer)`. Маппинг ручек → параметры ctor (`FileTarget.cs:27-34`): `RollEvery`→`fileSizeLimitBytes`, `Keep`→`retainedFileCountLimit`, `AutoFlush`→`autoFlush`. В `Build()` кинуть `InvalidOperationException`, если `_path` пустой.
- `src/Runtime/Targets/Memory/MemoryTargetBuilder.cs` — `Capacity(int)`, `Format`, `Filter`, `Build() → new InMemoryTarget(_capacity, formatter, filterer)`. Кинуть `InvalidOperationException`, если `_capacity <= 0`.

### Изменить
- `src/Runtime/ULogger.Lifecycle.cs` — добавить `public static void Configure(Action<LoggerConfiguration>)` с порядком выше; упростить `Initialize()` до `Configure(_ => {});` (поведение идентично: хуки с нулём таргетов, дальше `AddTarget`). `Terminate()` не трогать.
- `src/Runtime/Targets/Filterer.cs` — добавить статические фабрики `public static Filterer AllowAllTags() => new(true);` и `public static Filterer BlockAllTags() => new(false);`. **Ctor `Filterer(bool)`, `SetAllowed`, `Enable/Disable/Allow/Mute` оставить публичными** (контракт `FiltererTests` + back-compat; zero-warnings — НЕ вешать `[Obsolete]`).

### Удалить
- `src/Runtime/Types/LoggerConfig.cs` — `public class LoggerConfig` (PlayerPrefs-тоглы тегов): **0 ссылок** по всему репо, `UnityFormatter`/`AddTarget` никем не зовутся, к `Filterer` не подключён. Мёртвый код — удалить.

### Мигрировать
- `Appegy.UniLogger.Lab/Assets/Scripts/ULoggerInitializer.cs` (`:45-56`) — заменить `Initialize()` + 2× `AddTarget(new ...)` на один `ULogger.Configure(c => c.WriteTo.Unity(...).WriteTo.File(...))`. Блок `Application.SetStackTraceLogType` (нативный стек Unity, отдельная концепция от `Target.SetStackTraceEnabled`) оставить как есть.

## Тесты (`src/Tests/`)

Существующие тесты init-API не трогают (юзают `ULoggerData` напрямую) → остаются зелёными. Добавить:
- `LoggerConfigurationTests.cs`:
  - materialization: `Configure` с Unity+File+Memory → `GetTargets()` содержит 3 таргета в порядке регистрации, корректных типов.
  - delivery: после `Configure` лог доходит до sync-таргета (переиспользовать pattern `RecordingTarget` из `ExceptionDeliveryTests.cs:9`).
  - atomicity/native capture: после `Configure` нативный `Debug.Log` ловится таргетом с `CapturesNativeLogs=true` (pattern из `NativeLogRoutingTests.cs`).
  - duplicate-type: два `.WriteTo.Unity(...)` → `InvalidOperationException`.
  - escape-hatch: `.WriteTo.Target(new CustomTarget())` регистрирует кастомный таргет.
- `FiltererFactoryTests.cs`: `Filterer.AllowAllTags()` ≡ `new Filterer(true)`, `Filterer.BlockAllTags()` ≡ `new Filterer(false)` (по `IsAllowed`); цепочки `AllowAllTags().Disable(level).Mute(tag)` и `BlockAllTags().Allow(tag)`.

## Доки (под-таска — делает @ivan в рамках этой задачи)

Обновить под новый API:
- `README.md` — секции **Quick start**, **Logs filtering** (`new Filterer(true/false)` → фабрики), **Logs formatting** (примеры через `.Format(...)` в builder'е), **Logs targets** (регистрация через `WriteTo`).
- `docs/logger-lifecycle.md`, `docs/formatting-and-filtering.md`, `docs/targets.md` — заменить placeholder'ы контентом под новый API.

Целевые примеры API — в разделе «Целевой публичный API» выше и в дизайне builder'ов.

## Out of scope / решения

- НЕ вешать `[Obsolete]` на ctor `Filterer(bool)` (zero-warnings + back-compat). Фабрики = рекомендуемый путь в доке.
- НЕ вводить flat-chain `.RichText()/.Tags()` на builder'ах — `.Format(FormatOptions.X|Y)` лаконичнее.
- `Target.SetStackTraceEnabled` (0 вызовов) не трогать; в builder его не выносить.
- `ULOGGER_DISABLE_ALL_LOGS` на init не спец-обрабатывать — parity с текущим поведением (`Lifecycle.cs` не имеет `#if`, init компилируется и работает; только emission-методы стрипаются). Проверить, что новый `Configure` тоже не ломается под этим define.

## Verify

1. **Unity MCP** (после КАЖДОГО C#-изменения): recompile → дождаться `isCompiling == false` → `read_console` (errors+warnings). Zero-warnings policy — фиксить всё. На API-изменениях проверять весь проект (Lab/`Assembly-Csharp`), не только пакет.
2. `run_tests` (EditMode) через MCP: новые + существующие (`FiltererTests`, `ULoggerDataTests`, `NativeLogRoutingTests`, `ExceptionDeliveryTests`, `FileTargetTests`, `InMemoryTargetTests`) — все зелёные.
3. Play-mode дым в Lab: войти в Play, убедиться что логи `GameManager`/`Debug.Log` идут в Unity-консоль с форматированием и в rolling-файл (`Appegy.UniLogger.Lab/Logs/game.log`), без потери и без дублей.
4. `dotnet build`/Rider для nullable-предупреждений (консоль Unity часть пропускает — см. CLAUDE.md).
