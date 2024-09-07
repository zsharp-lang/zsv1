using CommonZ.Utils;

using Scope = CommonZ.Utils.Cache<string, ZSharp.Compiler.CGObject>;

namespace ZSharp.Compiler
{
    public sealed partial class Context(Scope globalScope)
    {
        private readonly Mapping<CGObject, Scope> scopes = [];

        public Scope CurrentScope { get; private set; } = globalScope;

        public Scope GlobalScope { get; } = globalScope;

        public ContextManager For(CGObject context, Scope? parent = null)
        {
            if (!scopes.TryGetValue(context, out var scope))
                scope = scopes[context] = new()
                {
                    Parent = parent ?? CurrentScope
                };

            (CurrentScope, scope) = (scope, CurrentScope);

            return new(() => CurrentScope = scope);
        }

        public ContextManager Scope(Scope? parent = null)
        {
            Scope scope = new()
            {
                Parent = parent ?? CurrentScope
            };

            (CurrentScope, scope) = (scope, CurrentScope);

            return new(() => CurrentScope = scope);
        }
    }
}
