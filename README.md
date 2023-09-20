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
- [Logger tags](#logger-tags)
- [Logs filtering](#logs-filtering)
- [Logs formatting](#logs-formatting)
- [Logs targets](#logs-targets)
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

I strongly discourage using `Debug.Log`, `Debug.LogWarning` and `Debug.LogError` in conjunction with `ULogger`. Despite the fact that all logs sent to these methods will be tagged, formatted, and sent to targets, there will be no ability to eliminate strings construction when using the methods `Log`, `LogWarning`, and `LogError`. In other words, even if `Debug.Log($"Add {count} coins")` is called and nothing appears in the console (in the case of logs deactivation), the string will still be constructed, which may slightly impact performance.

## Quick start

First of all you have to initialize `ULogger`. To do this you should call the `ULogger.Initialize(Formatter, Filterer)` method. It's best to do this at the very start of your application, before any logs are generated, to ensure that nothing is missed. The most effective way to achieve this is by using a static method with the [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)](https://docs.unity3d.com/ScriptReference/RuntimeInitializeLoadType.SubsystemRegistration.html) attribute.

During initialization, you will need to configure the Formatter and Filterer to be used for logs sent to the Unity console. You can learn how to [format](#logs-formatting) and [filter](#logs-filtering) logs in the respective sections.

```C#
using UnityEngine;

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

![example](.images/01_quickstart_example.png)

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

![example](.images/02_multi_tagging.png)


## Logs filtering

TODO

## Logs formatting

TODO

## Logs targets

TODO

## Removing logs from release builds

TODO

## Why do I use UnityEngine namespace

TODO
