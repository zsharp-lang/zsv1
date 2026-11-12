using System.Diagnostics.CodeAnalysis;

namespace CommonZ
{
    /// <summary>
    /// Why something failed. What an error can say beyond that is a capability it has,
    /// found as a CO's are: an error that wraps another, as one with where it happened
    /// would, has the capabilities of the one it wraps.
    /// </summary>
    public abstract class Error
    {
        public virtual bool Is<T>([NotNullWhen(true)] out T? capability)
            where T : class
            => (capability = this as T) is not null;
    }
}
