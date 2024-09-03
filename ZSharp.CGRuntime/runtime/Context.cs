using CommonZ.Utils;
using Scope = CommonZ.Utils.Cache<string, ZSharp.CGRuntime.CGObject>;

namespace ZSharp.CGRuntime
{
    public sealed class Context
    {
        private readonly Mapping<CGObject, Scope> objectScopes = [];
        private readonly Stack<Scope> scopeStack = [];

        public Scope CurrentScope => scopeStack.Peek();

        public Scope GlobalScope { get; } = new();

        public Context()
        {
            scopeStack.Push(GlobalScope);
        }

        public CGObject? Get(string name)
            => CurrentScope.Cache(name);

        public bool Set(string name, CGObject value)
            => !CurrentScope.Contains(name)
            && CurrentScope.Cache(name, value) is not null;

        public bool Del(string name)
            => CurrentScope.Uncache(name);

        public void Enter(CGObject @object)
        {
            if (!objectScopes.TryGetValue(@object, out var scope))
                objectScopes[@object] = scope = new()
                {
                    Parent = CurrentScope
                };

            scopeStack.Push(scope);
        }

        public void Leave()
            => scopeStack.Pop();

        public void PutScope(Scope scope)
            => scopeStack.Push(scope);

        public void PopScope()
            => scopeStack.Pop();
    }
}
