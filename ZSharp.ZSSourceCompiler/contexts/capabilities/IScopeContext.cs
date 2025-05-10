using System.Diagnostics.CodeAnalysis;
using ZSharp.Compiler;

namespace ZSharp.ZSSourceCompiler
{
    public interface IScopeContext
        : IContext
    {
        public CompilerObject? Get(string name);

        public T? Get<T>(string name)
            where T : class, CompilerObject;

        public bool Get(string name, [NotNullWhen(true)] out CompilerObject? result);

        public bool Get<T>(string name, [NotNullWhen(true)] out T? result)
            where T : class, CompilerObject;

        public void Set(string name, CompilerObject @object);
    }
}
