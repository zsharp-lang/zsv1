using CommonZ.Utils;
using System.Diagnostics.CodeAnalysis;
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

        public T? Get<T>(string name)
            where T : class, CompilerObject
            => Get(name) is T result ? result : null;

        public bool Get(string name, [NotNullWhen(true)] out CompilerObject? result)
            => (result = Get(name)) is not null;

        public bool Get<T>(string name, [NotNullWhen(true)] out T? result) 
            where T : class, CompilerObject
            => (result = Get<T>(name)) is not null;

        public void Set(string name, CompilerObject @object)
            => scope[name] = @object;
    }
}
