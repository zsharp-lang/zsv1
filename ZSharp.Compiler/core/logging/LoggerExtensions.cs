using ZSharp.Logging;

namespace ZSharp.Compiler
{
    public static class LoggerExtensions
    {
        public static void Info<T>(this Logger<T> logger, T message, CompilerObject origin)
            => logger.Log(new() { Message = message, Level = LogLevel.Info, Origin = Origin(origin) });

        public static void Warning<T>(this Logger<T> logger, T message, CompilerObject origin)
            => logger.Log(new() { Message = message, Level = LogLevel.Warning, Origin = Origin(origin) });

        public static void Error<T>(this Logger<T> logger, T message, CompilerObject origin)
            => logger.Log(new() { Message = message, Level = LogLevel.Error, Origin = Origin(origin) });

        public static LogOrigin Origin(CompilerObject @object)
            => new ObjectLogOrigin(@object);
    }
}
