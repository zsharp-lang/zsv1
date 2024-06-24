using CommonZ.Utils;
using ZSharp.RAST;

namespace ZSharp.CTRuntime
{
    public sealed class DomainContext<T>(Cache<RNode, IBinding<T>>? rootScope)
        where T : class
    {
        public Cache<RNode, Cache<RNode, IBinding<T>>> Scopes { get; } = new();

        public Cache<RNode, IBinding<T>> Scope { get; private set; } = rootScope ?? new();

        public ContextManager NewScope(RNode node)
        {
            var scope = Scope;

            (scope, Scope) = (Scope, new Cache<RNode, IBinding<T>>() { Parent = Scope });

            Scopes.Cache(node, Scope);

            return new(() =>
            {
                Scope = scope;
            });
        }

        public ContextManager UseScope(RNode node)
        {
            Cache<RNode, IBinding<T>> scope;
            (scope, Scope) = (Scope, Scopes.Cache(node) ?? Scopes.Cache(node, new Cache<RNode, IBinding<T>>() { Parent = Scope }));

            return new(() =>
            {
                Scope = scope;
            });
        }
    }
}
