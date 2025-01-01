
namespace ZSharp.ZSSourceCompiler
{
    public sealed class WhileLoopCompiler(ZSSourceCompiler compiler, WhileExpression node)
        : ContextCompiler<WhileExpression, WhileLoop>(compiler, node, new())
        , IOverrideCompileStatement
    {
        public override WhileLoop Compile()
        {
            Object.Condition = Compiler.CompileNode(Node.Condition);

            using (Context.Compiler(this))
            using (Context.Scope())
                Object.While = Compiler.CompileNode(Node.Body);

            if (Node.Else is not null)
                using (Context.Scope())
                    Object.Else = Compiler.CompileNode(Node.Else);

            return base.Compile();
        }

        public CompilerObject? CompileNode(ZSSourceCompiler compiler, Statement node)
        {
            if (node is BreakStatement @break)
                return new Objects.RawCode(
                    new([
                        .. @break.Value is null ? [] : compiler.Compiler.CompileIRCode(
                            compiler.CompileNode(@break.Value)
                        ).Instructions,

                        new IR.VM.Jump(Object.EndLabel)
                    ])
                );

            return null;
        }
    }
}
