namespace ZSharp.Compiler
{
    public sealed class Logger<T>
    {
        private readonly List<Log<T>> logs = [];

        public IReadOnlyList<Log<T>> Logs => logs.AsReadOnly();

        public void Log(Log<T> log)
            => logs.Add(log);

        public void Info(T message, LogOrigin origin)
            => Log(new () { Message = message, Level = LogLevel.Info, Origin = origin });

        public void Warning(T message, LogOrigin origin)
            => Log(new() { Message = message, Level = LogLevel.Warning, Origin = origin });

        public void Error(T message, LogOrigin origin)
            => Log(new() { Message = message, Level = LogLevel.Error, Origin = origin });

        public void Info(T message, CompilerObject origin)
            => Log(new() { Message = message, Level = LogLevel.Info, Origin = Origin(origin) });

        public void Warning(T message, CompilerObject origin)
            => Log(new() { Message = message, Level = LogLevel.Warning, Origin = Origin(origin) });

        public void Error(T message, CompilerObject origin)
            => Log(new() { Message = message, Level = LogLevel.Error, Origin = Origin(origin) });

        public LogOrigin Origin(CompilerObject @object)
            => new ObjectLogOrigin(@object);
    }
}
