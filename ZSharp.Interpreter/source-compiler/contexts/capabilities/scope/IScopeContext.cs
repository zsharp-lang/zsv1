using ZSharp.Compiler;

namespace ZSharp.ZSSourceCompiler
{
    public interface IScopeContext
        : IContext
        , ILookupContext
    {
        public void Set(string name, CompilerObject @object);
    }
}
