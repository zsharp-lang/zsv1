using System.Diagnostics.CodeAnalysis;

namespace ZSharp.Compiler
{
    public interface IContext
    {
        private static IContextChainStrategy DefaultStrategy { get; } = new ParentContextStrategy();

        public IContext? Parent { get; internal protected set; }

        public T? FindContext<T>(IContextChainStrategy? strategy = null)
            where T : class, IContext
        {
            strategy ??= DefaultStrategy;

            var context = this;

            do
            {
                if (context is T capability) return capability;
            } while ((context = strategy.NextContext(context!)) is not null);

            return null;
        }

        public bool PerformOperation<T>(
            Func<T, bool> operation,
            IContextChainStrategy? strategy = null
        )
            where T : class, IContext
        {
            strategy ??= DefaultStrategy;

            var context = this;

            do
            {
                if (context is T capability && operation(capability)) return true;
            } while ((context = strategy.NextContext(context!)) is not null);


            return false;
        }

        public bool PerformOperation<T>(
            Func<T, bool> operation,
            [NotNullWhen(true)] out T? ctx,
            IContextChainStrategy? strategy = null
        )
            where T : class, IContext
        {
            strategy ??= DefaultStrategy;

            var context = this;

            do
            {
                if (context is T capability && operation(capability)) return (ctx = capability) is not null;
            } while ((context = strategy.NextContext(context!)) is not null);

            return (ctx = null) is not null;
        }
    }
}
