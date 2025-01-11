namespace ZSharp.ZSSourceCompiler
{
    public sealed class WhileStatementCompiler(ZSSourceCompiler compiler, WhileExpression<Statement> node)
        : WhileLoopCompiler<Statement>(compiler, node, compiler.Compiler.TypeSystem.Void)
    {
        protected override CompilerObject CompileBreak(BreakStatement @break)
        {
            // TODO: add support for 'break from' statement

            if (@break.Value is not null)
                Compiler.LogError("Break statement in a while statement must not have a value", @break);

            return new Objects.RawCode(
                new([
                    new IR.VM.Jump(Object.EndLabel)
                ])
            );
        }

        protected override CompilerObject CompileElse()
                => Compiler.CompileNode(Node.Else!);
    }
}
