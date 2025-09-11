using System.Diagnostics.CodeAnalysis;

namespace ZSharp.Compiler
{
    public interface CompilerObject
    {
        public bool Is<T>([NotNullWhen(true)] out T? result)
            where T : class
            => (result = this as T) is not null;
    }
}
