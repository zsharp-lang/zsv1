using CommonZ.Utils;
using CG = ZSharp.CGRuntime;

using Scope = CommonZ.Utils.Cache<string, object>; // object = CGObject

namespace ZSharp.Compiler
{
    internal partial class IRGenerator
    {
        private readonly Mapping<CGObject, Scope> scopes = [];
        private readonly Stack<CGObject> contextStack = [];

        public Scope CurrentScope { get; private set; } = new();

        public CGObject? CurrentContext => contextStack.Count == 0 ? null : contextStack.Peek();

        public ContextManager ContextOf(CGObject @object)
        {
            contextStack.Push(@object);
            if (!scopes.TryGetValue(@object, out var scope))
                scopes[@object] = scope = new()
                {
                    Parent = CurrentScope
                };

            (CurrentScope, scope) = (scope, CurrentScope);

            return new(() =>
            {
                CurrentScope = scope;
                contextStack.Pop();
            });
        }
    }
}
