# UniLogger - Logger Lifecycle

> Placeholder. To be written.

**Scope to cover:** how the logger is set up and torn down. `Initialize`: stealing
`Debug.unityLogger.logHandler` (and storing the original), setting the project root for stack
relativization, creating the writer thread, subscribing to `logMessageReceivedThreaded`,
`UnobservedTaskException` and `quitting`, and the "no default targets" rule with the pre-init
fallback to `Debug.unityLogger`. `Terminate`: restoring the handler, unsubscribing, disposing the
dispatcher (final flush) and disposable targets. Editor specifics: `ULoggerPlayModeHandler`
terminating on `ExitingPlayMode` to flush the async tail before domain reload, and the
`[InitializeOnLoadMethod]` installer that registers the double-click handler and the drain loop.
Threading rules (`Initialize` / `AddTarget` / `Terminate` are main-thread only) and the compile-time
strip defines (`ULOGGER_*_OFF`, `ULOGGER_DISABLE_ALL_LOGS`). The per-log journey is in
[`logging-pipeline.md`](logging-pipeline.md).
