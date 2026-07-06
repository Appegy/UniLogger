# UniLogger - Targets

> Placeholder. To be written.

**Scope to cover:** each built-in target and how it turns a delivered entry into its sink, plus how
to author a custom one. `UnityTarget` (sync): the editor path that builds clickable console entries
via reflection (hyperlinked `(at Assets/...)`, double-click navigation, console `mode`/marker bits)
versus the player path that forwards to the native handler; background-thread marshaling to the main
thread; and why it opts out of native-log capture (`CapturesNativeLogs = false`) so native/engine
logs the Unity console already shows are not duplicated back into it. `FileTarget` (async): rolling by size, retention count, file naming, UTF-8, share mode and
the read-while-writing gotcha, editor vs `persistentDataPath` paths. `InMemoryTarget` (async): the
fixed ring buffer and `GetContent()`. Finally the `Target` base contract: `RunSynchronously`, `CapturesNativeLogs`
(opt out of native/engine log mirroring), the per-level stack toggle, `Log` / `LogException` /
`Flush`, and what a target may assume about the thread it runs on. How a log reaches these targets is in [`logging-pipeline.md`](logging-pipeline.md).
