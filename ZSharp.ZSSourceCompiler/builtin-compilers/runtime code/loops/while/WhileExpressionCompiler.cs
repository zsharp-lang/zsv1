namespace ZSharp.ZSSourceCompiler
{
    public sealed class WhileExpressionCompiler(ZSSourceCompiler compiler, WhileExpression<Expression> node, CompilerObject type)
        : WhileLoopCompiler<Expression>(compiler, node, type)
    {
        protected override CompilerObject CompileBreak(BreakStatement @break)
        {
            if (@break.Value is null)
            {
                Compiler.LogError("Break statement in a while expression must have a value", @break);
                return new Objects.RawCode(new([new IR.VM.Jump(Object.EndLabel)]));
            }

            var value = Compiler.CompileNode(@break.Value);
            if (!Compiler.Compiler.TypeSystem.IsTyped(value, out var type))
            {
                Compiler.LogError("Value must be a valid RT value!", @break.Value);
                return new Objects.RawCode(new([new IR.VM.Jump(Object.EndLabel)]));
            }

            Object.Type ??= type;

            var code = Compiler.Compiler.CompileIRCode(Compiler.Compiler.Cast(value, type));

            return new Objects.RawCode(
                new([
                    .. code.Instructions,

                    new IR.VM.Jump(Object.EndLabel)
                ])
            );
        }

        protected override CompilerObject CompileElse()
        {
            if (Object.Type is null)
            {
                Compiler.LogError("While expression must have a type", Node);
                return new Objects.RawCode(new([new IR.VM.Jump(Object.EndLabel)]));
            }

            return Compiler.Compiler.Cast(Compiler.CompileNode(Node.Else!), Object.Type);
        }
    }
}
