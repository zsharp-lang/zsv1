namespace ZSharp.ZSSourceCompiler
{
    public sealed class WhileExpressionCompiler(ZSSourceCompiler compiler, WhileExpression<Expression> node, Compiler.IType type)
        : WhileLoopCompiler<Expression>(compiler, node, type)
    {
        protected override ObjectResult CompileBreak(BreakStatement @break)
        {
            if (@break.Value is null)
                return Compiler.CompilationError("Break statement in a while expression must have a value", @break);

            var valueResult = Compiler.CompileNode(@break.Value);
            if (
                valueResult
                .When(out var value)
                .Error(out var error)
            )
                return ObjectResult.Error(error);
            if (!Compiler.Compiler.TypeSystem.IsTyped(value!, out var type))
                return Compiler.CompilationError("Value must be a valid RT value!", @break.Value);

            Object.Type ??= type;

            if (!Compiler.Compiler.TypeSystem.ImplicitCast(value, Object.Type).Ok(out var breakValue))
                return Compiler.CompilationError("Could not cast break value to while expression type", @break.Value);

            var code = Compiler.Compiler.IR.CompileCode(breakValue).Unwrap();

            return ObjectResult.Ok(new Objects.RawCode(
                new([
                    .. code.Instructions,

                    new IR.VM.Jump(Object.EndLabel)
                ])
            ));
        }

        protected override ObjectResult CompileElse()
        {
            if (Object.Type is null)
                return Compiler.CompilationError("While expression must have a type", Node);

            return ObjectResult.Ok(Compiler.Compiler.TypeSystem.ImplicitCast(Compiler.CompileNode(Node.Else!).Unwrap(), Object.Type).Unwrap());
        }
    }
}
