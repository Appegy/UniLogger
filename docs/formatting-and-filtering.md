# UniLogger - Formatting and Filtering

> Placeholder. To be written.

**Scope to cover:** the two per-target services that shape a log before it is written. `Filterer`:
level enable/disable plus tag allow/mute, the "all tags allowed by default" mode, and the immutable
snapshot model used for thread-safe reads. `Formatter`: the `FormatOptions` flags (`None`, `Time`,
`Thread`, `Tags`, `LogType`, `RichText`), how a line is assembled (time, type, thread, tags,
message), rich-text vs plain output, per-tag color derivation, and the color of a colored message.
Include the reference tables that belong here: log levels and their short names (`TR` / `LG` / `WN` /
`ER`), the `LogLevel` to `LogType` mapping, and a rendered-line example. Where filtering and
formatting sit in a log's journey is summarized in [`logging-pipeline.md`](logging-pipeline.md).
