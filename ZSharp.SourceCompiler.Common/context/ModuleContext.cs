using CommonZ.Utils;
using ZSharp.AST;

namespace ZSharp.SourceCompiler
{
    internal sealed class ModuleContext
        : ZSharp.Compiler.IContext
    {
        private Scope currentScope = null!;

        public required Scope CurrentScope
        {
            get => currentScope;
            init => currentScope = value;
        }

        public required Cache<Type, Func<IContext, Node, IResult>> Overrides { get; init; }
        public ZSharp.Compiler.IContext? Parent { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public ContextManager Scope(out Scope scope)
            => Scope(scope = CurrentScope.CreateChildScope());

        public ContextManager Scope(Scope scope)
        {
            (scope, currentScope) = (currentScope, scope);

            return new(() => { currentScope = scope; });
        }
    }
}
