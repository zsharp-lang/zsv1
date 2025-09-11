using System.Diagnostics.CodeAnalysis;
using ZSharp.Compiler;

namespace ZSharp.ZSSourceCompiler
{
    public interface ILookupContext
        : IContext
    {
        public CompilerObject? Get(string name);

        public T? Get<T>(string name)
            where T : class, CompilerObject
            => Get(name) is T value ? value : null;

        public bool Get(string name, [NotNullWhen(true)] out CompilerObject? result)
            => (result = Get(name)) is not null;

        public bool Get<T>(string name, [NotNullWhen(true)] out T? result)
            where T : class, CompilerObject
            => (result = Get<T>(name)) is not null;
    }
}
