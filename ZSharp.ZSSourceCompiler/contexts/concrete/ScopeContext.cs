using CommonZ.Utils;
using ZSharp.Compiler;

namespace ZSharp.ZSSourceCompiler
{
    public sealed class ScopeContext()
        : IContext
        , IScopeContext
    {
        private readonly Mapping<string, CompilerObject> scope = [];

        public IContext? Parent { get; set; }

        public CompilerObject? Get(string name)
            => scope.TryGetValue(name, out var result) ? result : null;
        public void Set(string name, CompilerObject @object)
            => scope[name] = @object;
    }
}
