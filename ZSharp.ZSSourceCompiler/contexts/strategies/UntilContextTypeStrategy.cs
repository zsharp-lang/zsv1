using ZSharp.Compiler;

namespace ZSharp.ZSSourceCompiler
{
    public sealed class UntilContextTypeStrategy<T>(
        IContextChainStrategy strategy
    )
        : IContextChainStrategy
        where T : IContext
    {
        public IContextChainStrategy Strategy { get; } = strategy;

        IContext? IContextChainStrategy.NextContext(IContext context)
        {
            if (context is T) return null;

            return Strategy.NextContext(context);
        }
    }
}
