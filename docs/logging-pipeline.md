# UniLogger - Log Processing Pipeline

This document traces the life of a single log in UniLogger: from the call that creates it to the
moment it reaches a target (`Log` or `LogException`). Every box in the diagrams maps to real code.

---

## Table of contents

1. [Big picture](#1-big-picture)
2. [The core: `SendLog`](#2-the-core-sendlog)
3. [Sync vs async and the writer thread](#3-sync-vs-async-and-the-writer-thread)
4. [Exceptions](#4-exceptions)
5. [Stack-trace handling](#5-stack-trace-handling)
6. [Recursion guard and the native feedback loop](#6-recursion-guard-and-the-native-feedback-loop)

---

## 1. Big picture

UniLogger takes over Unity's log handler in `Initialize()`. After that, every `Debug.Log*` and
`Debug.LogException` in the project goes through the same path as `ULogger` calls. UniLogger also
builds and cleans the call stack itself, so there is no extra trip through Unity to get it.

Each log goes to one of two kinds of target. **Sync** targets get the log at once, on the same
thread that logged it. **Async** targets go to a queue that one background thread writes out. A
target picks its kind with `RunSynchronously`.

```mermaid
flowchart TD
    subgraph Sources["Log sources"]
        A1["ULogger.Log*"]
        A2["ULogger.LogException(ex)"]
        A3["Debug.Log*"]
        A4["Debug.LogException / uncaught exceptions"]
        A5["TaskScheduler.UnobservedTaskException"]
        A6["native / engine logs<br/>(Application.logMessageReceivedThreaded)"]
    end

    A1 --> S["ULogger.SendLog"]
    A3 --> H["UnityLogger.LogFormat<br/>(stolen logHandler)"] --> S
    A2 --> DE["ULogger.DispatchException"]
    A4 --> HE["UnityLogger.LogException"] --> DE
    A5 --> DE
    A6 --> BL["ULogger.BroadcastUnobservedLog"]

    S --> CORE{{"ULoggerData<br/>SyncTargets / AsyncTargets"}}
    DE --> CORE

    CORE -->|"sync - caller thread"| ST["Sync target(s)"]
    CORE -->|"async"| AT["LogDispatcher queue"]
    BL --> CORE
    AT -->|"writer thread"| ATT["Async target(s)"]
```

Native and engine logs (not from `ULogger` or `Debug`) are caught by `logMessageReceivedThreaded`
and sent to the targets that accept native logs. Some targets opt out of these (see
[`targets.md`](targets.md)).

---

## 2. The core: `SendLog`

`SendLog` is the main path for normal logs.

```mermaid
flowchart TD
    START["ULogger.Log*"] --> NULLCHK{"Initialized?"}
    NULLCHK -->|"no"| FALLBACK["log via Debug.unityLogger"]
    NULLCHK -->|"yes"| ENTRY["build LogEntry"]
    ENTRY --> NEEDS{"stack trace needed?"}
    NEEDS -->|"yes"| CAP["capture and clean stack"]
    NEEDS -->|"no"| SKIP["no stack"]
    CAP --> SYNC["deliver to sync targets"]
    SKIP --> SYNC
    SYNC --> ASYNCCHK{"async targets?"}
    ASYNCCHK -->|"yes"| ENQ["queue for writer thread"]
    ASYNCCHK -->|"no"| DONE["done"]
    ENQ --> DONE
```

Key points:

- Before `Initialize()` there are no targets yet, so the log falls back to `Debug.unityLogger`.
  Nothing is lost at startup.
- The stack is captured **once**, and only if some target needs it for this level. All targets share
  that one string.
- Sync targets run on the calling thread, so a main-thread log stays on the main thread.
- Async targets get a copy of the log on the queue.
- Each target then applies its own filter and formatter before writing. See
  [`formatting-and-filtering.md`](formatting-and-filtering.md).

---

## 3. Sync vs async and the writer thread

A target is either sync or async (`RunSynchronously`). Sync targets get the log right away, on the
thread that logged it. Async targets go to a queue. One background thread, `UniLogger.Writer`, reads
the queue and writes to them.

```mermaid
flowchart TD
    P["any thread: enqueue(record)"] --> Q[["BlockingCollection queue"]]
    Q --> T["writer thread: Take() (blocks)"]
    T --> D["deliver to async targets"]
    D --> DR{"more in queue?"}
    DR -->|"yes"| D
    DR -->|"no"| FL["Flush() targets"]
    FL --> T
```

- The thread takes one record, then also takes any others already waiting, and flushes once after the
  batch. This is fast and avoids a flush per line.
- On shutdown the queue closes, the thread writes what is left and flushes one last time, so nothing
  in the queue is lost.

---

## 4. Exceptions

Exceptions take their own path. UniLogger unwraps them, formats them once (type, message, clean
stack), and calls `target.LogException` directly. This path skips the per-target filter and
formatter, because an exception has no tags and is already a full text block.

```mermaid
flowchart TD
    IN["ULogger.LogException"] --> NULLCHK{"Initialized?"}
    NULLCHK -->|"no"| FB["forward to Debug.unityLogger"]
    NULLCHK -->|"yes"| UNWRAP["unwrap single AggregateException"]
    UNWRAP --> FORMAT["format once: type, message, clean stack"]
    FORMAT --> SYNC["sync targets: LogException"]
    FORMAT --> ASYNC["async targets: queue"]
```

For the stack, UniLogger uses `Exception.StackTrace` when it is set (Mono format, which it converts),
or the current stack otherwise. Both go through the noise filter. See
[Stack-trace handling](#5-stack-trace-handling).

---

## 5. Stack-trace handling

Two parts shape a stack. Which one runs depends on the path.

Normal logs:

```mermaid
flowchart LR
    N1["extract current stack<br/>(already Unity format)"] --> N2["remove noise frames"]
```

Exceptions:

```mermaid
flowchart LR
    E1["Exception.StackTrace (Mono)"] --> E2["convert to Unity format,<br/>make path relative"] --> E3["remove noise frames"]
```

### Noise filtering (`RemoveNoiseFrames`)

It removes stack lines that start with a known prefix, matched at a frame boundary
(`. : ( space / +`). Prefixes are grouped so filters are easy to add or remove:

| Group | Removes frames from |
| --- | --- |
| UniLogger internals | `ULogger`, `ExtendedULogger`, `UnityTarget`, `UnityConsoleBridge`, `UnityExceptionFormatter`, `LogDispatcher` |
| Unity logging | `UnityEngine.StackTraceUtility`, `Debug`, `Logger`, `UnityLogger` |
| Assertions | `UnityEngine.Assertions` |
| Async machinery | `AsyncMethodBuilderCore`, `System.Threading`, `UnitySynchronizationContext` |

The boundary check avoids false hits: a user type like `Appegy.UniLogger.ULoggerFoo` is kept,
because the character after `ULogger` (`F`) is not a boundary.

### Mono to Unity conversion (exceptions only)

`Exception.StackTrace` is a Mono-format string. `new StackTrace(exception)` would lose the real throw
site across `await`, so UniLogger parses the string instead:

```
  at NS.Type.Method (args) [0x00000] in /proj/Assets/Foo.cs:23
        becomes
NS.Type:Method (args) (at Assets/Foo.cs:23)
```

The path is made relative to the project root. Frames outside the project keep the method name but
lose the file location. In a build the source paths do not match the device, so exception locations
are dropped there. This is expected.

---

## 6. Recursion guard and the native feedback loop

Sometimes the pipeline calls Unity's **native** log handler (a target's fallback, or the
raw-exception forward). That native write fires `Application.logMessageReceivedThreaded`, which
UniLogger listens to for native logs. Without a guard, a forwarded log would come back and reach the
async targets a second time.

`_suppressNativeCapture` (a `[ThreadStatic]` flag) stops this:

```mermaid
flowchart TD
    F["forward to native handler"] --> B["set _suppressNativeCapture = true"]
    B --> N["native LogFormat / LogException"]
    N --> EV["logMessageReceivedThreaded fires"]
    EV --> CH{"flag set?"}
    CH -->|"yes"| IGN["ignored - no double delivery"]
    CH -->|"no"| CAP2["captured (a real native log)"]
    N --> E["clear the flag (finally)"]
```

The flag is set and cleared in a `try/finally`, so it never stays stuck, and it is `[ThreadStatic]`,
so it does not leak between threads. In the editor the normal console path writes to the editor
console directly and never touches native, so this loop only happens on the native-forward path
(mostly in a player build).
