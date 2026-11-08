using System.Diagnostics.CodeAnalysis;

namespace ZSharp.Compiler
{
    /// <summary>
    /// The context of a definition being compiled: a bag of capabilities, checked with
    /// <see cref="Is"/>. Nothing cares what a context is, only what it can do.
    /// </summary>
    public interface Context
    {
        public bool Is<T>([NotNullWhen(true)] out T? result) where T : class
            => (result = this as T) is not null;
    }

    /// <summary>
    /// A context that offers the capabilities of the one it is in as well as its own.
    /// Delegating is a choice: a context can also offer only its own.
    /// </summary>
    public abstract class ChildContext(Context? parent)
        : Context
    {
        public Context? Parent { get; } = parent;

        public bool Is<T>([NotNullWhen(true)] out T? result) where T : class
        {
            if ((result = this as T) is not null)
                return true;

            if (Parent is not null)
                return Parent.Is(out result);

            return false;
        }
    }

    /// <summary>
    /// The stack of contexts. A meta-compiler pushes its context while it compiles.
    /// </summary>
    public sealed class Contexts
    {
        private readonly Stack<Context> stack = [];

        public Context? Current => stack.TryPeek(out var current) ? current : null;

        /// <summary>
        /// Whether the current context can do something.
        /// </summary>
        public bool Has<T>([NotNullWhen(true)] out T? capability) where T : class
        {
            capability = null;

            return Current is { } current && current.Is(out capability);
        }

        public T Within<T>(Context context, Func<T> compile)
        {
            stack.Push(context);

            try
            {
                return compile();
            }
            finally
            {
                stack.Pop();
            }
        }
    }
}
