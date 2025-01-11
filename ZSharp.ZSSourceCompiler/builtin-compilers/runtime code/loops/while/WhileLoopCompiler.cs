
namespace ZSharp.ZSSourceCompiler
{
    public abstract class WhileLoopCompiler<TElse>(ZSSourceCompiler compiler, WhileExpression<TElse> node, CompilerObject type)
        : ContextCompiler<WhileExpression<TElse>, WhileLoop>(compiler, node, new()
        {
            Type = type
        })
        , IOverrideCompileStatement
        where TElse : Node
    {
        public override WhileLoop Compile()
        {
            Object.Condition = Compiler.CompileNode(Node.Condition);

            using (Context.Compiler(this))
            using (Context.Scope())
                Object.While = Compiler.CompileNode(Node.Body);

            if (Node.Else is not null)
                using (Context.Scope())
                    Object.Else = CompileElse();

            return base.Compile();
        }

        protected abstract CompilerObject CompileBreak(BreakStatement @break);

        protected abstract CompilerObject CompileElse();

        public CompilerObject? CompileNode(ZSSourceCompiler compiler, Statement node)
            => node switch
            {
                BreakStatement @break => CompileBreak(@break),
                _ => null
            };
    }
}
