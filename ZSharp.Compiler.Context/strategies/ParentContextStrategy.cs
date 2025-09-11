namespace ZSharp.Compiler
{
    public sealed class ParentContextStrategy : IContextChainStrategy
    {
        IContext? IContextChainStrategy.NextContext(IContext context)
            => context.Parent;
    }
}
