using System.Diagnostics.CodeAnalysis;

namespace ZSharp.MIR
{
    public interface CompilerObject
    {
        public bool Is<T>([NotNullWhen(true)] out T? result)
            where T : class
            => (result = this as T) is not null;
    }
}
