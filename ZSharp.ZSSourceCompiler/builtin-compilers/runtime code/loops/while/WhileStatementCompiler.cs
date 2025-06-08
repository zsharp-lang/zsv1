namespace ZSharp.ZSSourceCompiler
{
    public sealed class WhileStatementCompiler(ZSSourceCompiler compiler, WhileExpression<Statement> node)
        : WhileLoopCompiler<Statement>(compiler, node, compiler.Compiler.TypeSystem.Void)
    {
        protected override ObjectResult CompileBreak(BreakStatement @break)
        {
            // TODO: add support for 'break from' statement

            if (@break.Value is not null)
                return Compiler.CompilationError("Break statement in a while statement must not have a value", @break);

            return ObjectResult.Ok(new Objects.RawCode(
                new([
                    new IR.VM.Jump(Object.EndLabel)
                ])
            ));
        }

        protected override ObjectResult CompileElse()
            => Compiler.CompileNode(Node.Else!);
    }
}
