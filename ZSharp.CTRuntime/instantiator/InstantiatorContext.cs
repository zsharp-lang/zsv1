using CommonZ.Utils;

namespace ZSharp.CTRuntime
{
    //internal sealed class InstantiatorContext(Cache<ScopeKey, ScopeItem>? rootScope)
    //{
    //    public Cache<ScopeKey, ScopeItem> Scope { get; private set; } = rootScope ?? new();

    //    public Cache<ScopeKey, Cache<ScopeKey, ScopeItem>> Scopes { get; } = new();

    //    public Action UseScope(ScopeKey definition)
    //    {
    //        Cache<ScopeKey, ScopeItem> scope = new() { Parent = Scope };
    //        Scopes.Cache(definition, scope);

    //        (scope, Scope) = (Scope, scope);

    //        return () =>
    //        {
    //            Scope = scope;
    //        };
    //    }
    //}
}
