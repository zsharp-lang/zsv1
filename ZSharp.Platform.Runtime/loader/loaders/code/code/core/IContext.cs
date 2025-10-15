using System.Diagnostics.CodeAnalysis;

namespace ZSharp.Platform.Runtime.Loaders
{
    public interface IContext
    {
        public bool Is<T>([NotNullWhen(true)] out T? context)
            where T : class, IContext
            => (context = this as T) != null;
    }
}
