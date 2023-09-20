<!-- omit from toc -->
# 📝 Optimized logger for Unity3d

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
- [Advanced logger initialization](#advanced-logger-initialization)
- [Logs filtering](#logs-filtering)
- [Logs formatting](#logs-formatting)
- [Existing targets](#existing-targets)
- [Custom targets](#custom-targets)
- [Removing logs from release builds](#removing-logs-from-release-builds)
- [Why do I use UnityEngine namespace](#why-do-i-use-unityengine-namespace)

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
  "com.appegy.unilogger": "https://github.com/appegy/unilogger.git",
  ...
},
```

## Forgot about Debug.Log

I strongly discourage using `Debug.Log`, `Debug.LogWarning` and `Debug.LogError` in conjunction with `ULogger`. Despite the fact that all logs sent to these methods will be tagged, formatted, and sent to targets, there will be no ability to eliminate string construction when using the methods `Log`, `LogWarning`, and `LogError`. In other words, even if `Debug.Log($"Add {count} coins")` is called and nothing appears in the console (in the case of logs deactivation), the string will still be constructed, which may slightly impact performance.

## Quick start

Для быстрого начала использования `ULogger`'а достаточно создать в папаке Resources `ULoggerConfigurator` (Assets → Create → ULogger → Configurator). В этом ассете можно активировать встроенные таргеты а также настроить их форматеры. По умолчанию включен только `Unity Target`.

С этого момента абсолюто все логи будут обрабатываться `ULogger`'ом, включая логи отправляемые через `Debug.Log`.

Example:
```C#
public class ExampleBehaviour : MonoBehaviour
{
    private static readonly ULogger _logger = ULogger.GetLogger("Example");

    private void Awake()
    {
        _logger.Trace("_logger: Trace");
        _logger.Log("_logger: Log");
        _logger.LogWarning("_logger: Warning");
        _logger.LogError("_logger: Error");

        ULogger.Trace("ULogger: Trace");
        ULogger.Log("ULogger: Log");
        ULogger.LogWarning("ULogger: Warning");
        ULogger.LogError("ULogger: Error");

        Debug.Log("Debug: Log");
        Debug.LogWarning("Debug: Warning");
        Debug.LogError("Debug: Error");
    }
}
```

![example](.images/01_quickstart_example.png)

## Advanced logger initialization

TODO

## Logs filtering

TODO

## Logs formatting

TODO

## Existing targets

<!-- omit from toc -->
### FileTarget

TODO

<!-- omit from toc -->
### InMemoryTarget

TODO

## Custom targets

TODO

## Removing logs from release builds

TODO

## Why do I use UnityEngine namespace

TODO
