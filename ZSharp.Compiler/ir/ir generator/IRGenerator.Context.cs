using CommonZ.Utils;
using ZSharp.CGRuntime;
using Scope = CommonZ.Utils.Cache<string, ZSharp.CGRuntime.CGObject>; // object = CGObject

namespace ZSharp.Compiler
{
    internal partial class IRGenerator
    {
        private readonly Mapping<CGObject, Scope> scopes = [];
        private readonly Mapping<CGObject, Scope> closures = [];
        private readonly Stack<CGObject> contextStack = [];

        public Scope CurrentScope => runtime.Context.CurrentScope;

        public CGObject? CurrentContext => contextStack.Count == 0 ? null : contextStack.Peek();

        public ContextManager ClosureOf(CGObject @object)
        {
            if (!closures.TryGetValue(@object, out var closure))
                closures[@object] = closure = CurrentScope;

            runtime.Context.PutScope(closure);

            return new(() => runtime.Context.PopScope());
        }

        public ContextManager ContextOf(CGObject @object)
        {
            contextStack.Push(@object);
            if (!scopes.TryGetValue(@object, out var scope))
                scopes[@object] = scope = new()
                {
                    Parent = CurrentScope
                };

            runtime.Context.PutScope(scope);

            return new(() =>
            {
                runtime.Context.PopScope();
                contextStack.Pop();
            });
        }

        public ContextManager CompileHandler(ICompileHandler handler)
        {
            (compileHandler, handler) = (handler, compileHandler);

            return new(() => compileHandler = handler);
        }
    }
}
