namespace UnityEngine
{
    public abstract class Target
    {
        public LoggerConfig Config { get; }
        public Formatter Formatter { get; }

        protected Target(LoggerConfig config, Formatter formatter)
        {
            Config = config;
            Formatter = formatter;
        }

        public virtual (string Massage, string StackTrace) FormatLog(LogEntry log, string stackTrace)
        {
            return (Formatter.Format(log), stackTrace);
        }

        public abstract void Log(string message, string stackTrace);

        public virtual bool NeedExtractStackTraceFor(LogType logType)
        {
            return Formatter.GetStackTraceLogType(logType) != StackTraceLogType.None;
        }

        public virtual void Flush()
        {
        }
    }
}