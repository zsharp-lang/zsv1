using CommonZ.Utils;

namespace ZSharp.SourceCompiler
{
    public interface IContext
    {
        public Cache<Type, Func<IContext, AST.Node, IResult>> Overrides { get; }

        public Scope CurrentScope { get; }

        public ContextManager Scope(out Scope scope)
            => Scope(scope = CurrentScope.CreateChildScope());

        public ContextManager Scope(Scope scope);

        public void Override<T>(Func<IContext, T, IResult> function)
            where T : AST.Node
            => Overrides.Cache(
                typeof(T), 
                (IContext context, AST.Node node) => function(context, (T)node), 
                set: true
            );

        public void AddTask(Action task);

        public CompilerObject CreateSlot();

        public void Define(CompilerObject definition);

        public void Emit(CompilerObject code);
    }
}
