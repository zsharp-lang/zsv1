using ZSharp.Logging;

namespace ZSharp.SourceCompiler
{
    public static class LoggerExtensions
    {
        public static void Error<T>(this Logger<T> logger, T message, AST.Node node)
            => logger.Error(message, new NodeLogOrigin(node));
    }
}
