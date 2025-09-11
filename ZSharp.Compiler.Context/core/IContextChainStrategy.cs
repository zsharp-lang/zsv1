namespace ZSharp.Compiler
{
    public interface IContextChainStrategy
    {
        public IContext? NextContext(IContext context);
    }
}
