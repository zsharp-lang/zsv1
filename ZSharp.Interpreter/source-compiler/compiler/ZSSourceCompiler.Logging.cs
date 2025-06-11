using ZSharp.Compiler;

namespace ZSharp.ZSSourceCompiler
{
    public sealed partial class ZSSourceCompiler
    {
        public void LogError<T>(string message, Node origin)
            where T : class
            => Compiler.Log.Error(message, new NodeLogOrigin(origin));

        public void LogError(string message, Node origin)
            => LogError<CompilerObject>(message, origin);
    }
}
