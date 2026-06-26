<!-- omit from toc -->
# 📝 Optimized logger for Unity

[![openupm](https://img.shields.io/npm/v/com.appegy.unilogger?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.appegy.unilogger/)

Logging is an essential component of project diagnostics during development and testing. However, it comes with a drawback: the more logs you have, the more challenging it becomes to locate the specific ones you need at any given moment.

Unity's standard `Debug.Log` has several limitations:

- It lacks the capability to attach tags to logs for future filtering.
- There is no option to selectively disable specific logs (e.g., excluding logs from the input system).
- Logs cannot be redirected to alternative destinations, such as files, Firebase, personal servers, etc.
- It is not possible to completely disable all logs in release builds. Even when disabled by configuring Debug.logger, log lines will still be generated during a Log call but will not be displayed.

All of these issues are addressed by ULogger.

<!-- omit from toc -->
## Table of content

- [Package installation](#package-installation)
- [Forgot about Debug.Log](#forgot-about-debuglog)
- [Quick start](#quick-start)
- [Logger tags](#logger-tags)
- [Logs filtering](#logs-filtering)
- [Logs formatting](#logs-formatting)
- [Logs targets](#logs-targets)
- [Removing logs from release builds](#removing-logs-from-release-builds)

## Package installation

<!-- omit from toc -->
### Using OpenUPM

Using [OpenUPM-CLI](https://openupm.com/docs/getting-started.html) run the command

```
openupm add com.appegy.unilogger
```

Alternatively, you can install the package manually by following the instructions on the package [page](https://openupm.com/packages/com.appegy.unilogger/).

<!-- omit from toc -->
### Using Git link

Add package to the ```manifest.json```.

```json
"dependencies": {
  "com.appegy.unilogger": "https://github.com/appegy/unilogger.git?path=/src",
  ...
},
```

## Forgot about Debug.Log

I strongly discourage using `Debug.Log`, `Debug.LogWarning` and `Debug.LogError` in conjunction with `ULogger`. Despite the fact that all logs sent to these methods will be tagged, formatted, and sent to targets, there will be no ability to eliminate strings construction when using the methods `Log`, `LogWarning`, and `LogError`. In other words, even if `Debug.Log($"Add {count} coins")` is called and nothing appears in the console (in the case of logs deactivation), the string will still be constructed, which may slightly impact performance.

## Quick start

First of all you have to initialize `ULogger`. To do this you should call the `ULogger.Initialize(Formatter, Filterer)` method. It's best to do this at the very start of your application, before any logs are generated, to ensure that nothing is missed. The most effective way to achieve this is by using a static method with the [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)](https://docs.unity3d.com/ScriptReference/RuntimeInitializeLoadType.SubsystemRegistration.html) attribute.

During initialization, you will need to configure the Formatter and Filterer to be used for logs sent to the Unity console. You can learn how to [format](#logs-formatting) and [filter](#logs-filtering) logs in the respective sections.

```C#
using UnityEngine;
using Appegy.UniLogger;

public static class ULoggerInitializer
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void AutoConfigureLogger()
    {
        // Customize logs formatter for unity target
        var formatter = Application.isEditor
            ? new Formatter(FormatOptions.RichText | FormatOptions.Tags)
            : new Formatter(FormatOptions.Tags | FormatOptions.LogType);

        // Prepare filterer for unity target (by default all logs are allowed)
        var filterer = new Filterer(true);

        // When formatter and filterer are ready - initialize logger 
        ULogger.Initialize(formatter, filterer);
    }
}
```

From this point forward, all logs will be processed by `ULogger`, including logs sent via Debug.Log.

Example:

```C#
using UnityEngine;
using Appegy.UniLogger;

public class ExampleBehaviour : MonoBehaviour
{
    private static readonly ULogger _logger = ULogger.GetLogger("Example");

    private void Awake()
    {
        // All logs sent by _logger will have "Example" tag
        _logger.Trace("_logger: Trace");
        _logger.Log("_logger: Log");
        _logger.LogWarning("_logger: Warning");
        _logger.LogError("_logger: Error");

        // All logs sent by ULogger will have "Unsorted" tag
        ULogger.Trace("ULogger: Trace");
        ULogger.Log("ULogger: Log");
        ULogger.LogWarning("ULogger: Warning");
        ULogger.LogError("ULogger: Error");

        // All logs sent by Debug will have "Unsorted" tag
        // Despite the fact that this calls will work, I don't recommend to use Debug anymore
        Debug.Log("Debug: Log");
        Debug.LogWarning("Debug: Warning");
        Debug.LogError("Debug: Error");
    }
}
```

<p align="center">
  <img src="images/01_quickstart_example_dt.png#gh-dark-mode-only">
  <img src="images/01_quickstart_example_lt.png#gh-light-mode-only">
</p>

## Logger tags

When creating an instance of `ULogger`, you can pass any number of objects as parameters. Later all parameters will be converted to tags. Each log sent to logger instance will be marked with all of these tags. This allows you to filter logs into different targets later on. For most cases, the result of method `ToString()` will be used as the tag. However, there are a few exceptions:
- If `null` is sent as a parameter, the tag will be the string `NULL`.
- If a `Type` with the `TagName` attribute is sent as a parameter, the tag will be the string provided in the attribute's constructor.
- If a `Type` without the `TagName` attribute is sent as a parameter, the tag will be the result of the `Type.Name` property.
- If an `Enum` with the `TagName` attribute is sent as a parameter, the tag will be the string provided in the attribute's constructor.
- If an `Enum` without the `TagName` attribute is sent as a parameter, the tag will be the result of the `ToString()` method.

Example:
```C#
using UnityEngine;
using Appegy.UniLogger;

[TagName("TAG_TYPE")]
public class ExampleBehaviour : MonoBehaviour
{
    public enum Tags
    {
        Tag1,
        [TagName("TAG_2")]
        Tag2,
    }

    private static readonly ULogger _logger = ULogger.GetLogger(
        null,                     // [NULL]
        Tags.Tag1,                // [Tag1]
        Tags.Tag2,                // [TAG_2]
        "TAG_STR",                // [TAG_STR]
        typeof(Object),           // [Object]
        typeof(ExampleBehaviour), // [TAG_TYPE]
        15                        // [15]
    );

    private void Awake()
    {
        _logger.Log("Hello!");
    }
}
```

<p align="center">
  <img src="images/02_multi_tagging_dt.png#gh-dark-mode-only">
  <img src="images/02_multi_tagging_lt.png#gh-light-mode-only">
</p>


## Logs filtering

A `Filterer` decides which logs a target accepts. A log is accepted only when both its level and its tag are allowed.

Choose the default tag policy in the constructor:

- `new Filterer(true)` - opt-out: every tag is allowed until you disable it.
- `new Filterer(false)` - opt-in: every tag is denied until you enable it.

```C#
var filterer = new Filterer(true);

// Disable an entire level
filterer.SetAllowed(LogLevel.Trace, false);

// Mute a noisy tag
filterer.SetAllowed("Input", false);
```

```C#
// Opt-in: deny everything, then allow only what you need
var whitelist = new Filterer(false);
whitelist.SetAllowed("Network", true);
whitelist.SetAllowed("Save", true);
```

A log with several tags is accepted if at least one of its tags is allowed, and only the allowed tags are shown. The filterer you pass to `ULogger.Initialize` controls the Unity console.

## Logs formatting

A `Formatter` turns a log into its final string. Build one from `FormatOptions` flags (combine with `|`):

| Option | Effect |
| --- | --- |
| `None` | Raw message, no decoration |
| `RichText` | Enables color: per-tag colors, the level color, and any explicit message color |
| `Time` | Prefixes the timestamp as `[HH:mm:ss:fff]` |
| `Thread` | Prefixes the managed thread id as `[TH=<id>]` |
| `Tags` | Prefixes every tag as `[Tag]` |
| `LogType` | Prefixes the level as `[TR]` / `[LG]` / `[WN]` / `[ER]` / `[EX]` |

```C#
// Verbose, colored output (handy in the editor)
var editor = new Formatter(FormatOptions.RichText | FormatOptions.Tags | FormatOptions.Time | FormatOptions.LogType);

// Compact, plain output (handy in builds)
var build = new Formatter(FormatOptions.Tags | FormatOptions.LogType);
```

Tag colors are derived deterministically from the tag name, so a given tag always gets the same color across runs and platforms. `RichText` must be enabled for any coloring to appear.

## Logs targets

A target is where formatted logs are written. Each target owns its own `Formatter` and `Filterer`, so formatting and filtering are decided per destination.

The built-in `UnityTarget` writes to the Unity console. You configure it when you initialize the logger - the `formatter` and `filterer` you pass become the console target's settings:

```C#
ULogger.Initialize(formatter, filterer);
```

You can inspect the active targets at runtime:

```C#
foreach (var target in ULogger.GetTargets()) { /* ... */ }
var unityTarget = ULogger.GetTarget<UnityTarget>();
```

> Routing logs to additional destinations (files, remote services, and so on) is on the roadmap and not yet part of the public API. Today the Unity console is the active target.

## Removing logs from release builds

ULogger can remove logging at compile time. Disabled calls are marked with `[Conditional]`, so the compiler strips the entire call, including its argument expressions. An interpolated string like `$"Spawned {count} enemies"` is never built, so there is no string allocation and no runtime cost.

Add the matching Scripting Define Symbol (Project Settings -> Player -> Scripting Define Symbols):

| Define | Effect |
| --- | --- |
| `ULOGGER_TRACE_OFF` | strips `Trace` calls |
| `ULOGGER_LOGS_OFF` | strips `Log` calls |
| `ULOGGER_WARNINGS_OFF` | strips `LogWarning` calls |
| `ULOGGER_ERRORS_OFF` | strips `LogError` calls |
| `ULOGGER_DISABLE_ALL_LOGS` | strips all of the above at once |

`LogException` is never stripped, so exceptions are always reported.

This is compile-time removal: the code is gone from the build. To toggle logs while the game runs, filter them at runtime with a `Filterer` instead (see [Logs filtering](#logs-filtering)).
