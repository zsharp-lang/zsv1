using CommonZ.Utils;
using ZSharp.CGRuntime;
using Scope = CommonZ.Utils.Cache<string, ZSharp.CGRuntime.CGObject>;

namespace ZSharp.Compiler
{
    internal partial class IRGenerator
    {
        private readonly Mapping<CGObject, Scope> scopes = [];
        private readonly Stack<CGObject> contextStack = [];

        public Scope CurrentScope => CG.Context.CurrentScope;

        public CGObject? CurrentContext => contextStack.Count == 0 ? null : contextStack.Peek();

        public T? FindContext<T>()
            where T : class
        {
            foreach (var item in contextStack)
                if (item is T protocol)
                    return protocol;

            return null;
        }

        public ContextManager ContextOf(CGObject @object)
        {
            contextStack.Push(@object);
            if (!scopes.TryGetValue(@object, out var scope))
                scopes[@object] = scope = new()
                {
                    Parent = CurrentScope
                };

            CG.Context.PutScope(scope);

            return new(() =>
            {
                CG.Context.PopScope();
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
