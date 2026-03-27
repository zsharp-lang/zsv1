using System.Diagnostics.CodeAnalysis;

namespace ZSharp.Compiler
{
    public interface CompilerObject
    {
        public sealed T? As<T>()
            where T : class
            => Is<T>(out var result) ? result : null;

        public bool Is<T>([NotNullWhen(true)] out T? result)
            where T : class
            => (result = this as T) is not null;
    }
}
