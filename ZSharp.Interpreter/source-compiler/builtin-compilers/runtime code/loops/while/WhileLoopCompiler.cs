
namespace ZSharp.ZSSourceCompiler
{
    public abstract class WhileLoopCompiler<TElse>(ZSSourceCompiler compiler, WhileExpression<TElse> node, Compiler.IType type)
        : ContextCompiler<WhileExpression<TElse>, WhileLoop>(compiler, node, new()
        {
            Type = type
        })
        , IOverrideCompileStatement
        where TElse : Node
    {
        public override WhileLoop Compile()
        {
            Object.Condition = Compiler.CompileNode(Node.Condition).Unwrap();

            using (Context.Compiler(this))
            using (Context.Scope())
                Object.While = Compiler.CompileNode(Node.Body).Unwrap();

            if (Node.Else is not null)
                using (Context.Scope())
                    Object.Else = CompileElse().Unwrap();

            return base.Compile();
        }

        protected abstract ObjectResult CompileBreak(BreakStatement @break);

        protected abstract ObjectResult CompileElse();

        public ObjectResult? CompileNode(ZSSourceCompiler compiler, Statement node)
            => node switch
            {
                BreakStatement @break => CompileBreak(@break),
                _ => null
            };
    }
}
