namespace ZSharp.SourceCompiler
{
    public sealed class TopLevelExpressionCompiler(Interpreter.Interpreter interpreter)
    {
        public Interpreter.Interpreter Interpreter { get; } = interpreter;

        public Func<AST.Expression, Result> CompileExpression { get; init; } = new ExpressionCompiler(interpreter).Compile;

        public Func<object?, Result> LoadCO { get; init; } = @object => Result.Ok(ZSharp.Interpreter.CTServices.InfoOf(@object!));

        public Result Compile(AST.Expression expression)
        {
            var compileResult = CompileExpression(expression);

            if (
                compileResult
                .When(out var co)
                .IsError
            ) return compileResult;

            var evaluateResult = Interpreter.Evaluate(co!);

            if (
                evaluateResult
                .When(out var value)
                .Error(out var error)
            ) return Result.Error(error);

            return LoadCO(value);
        }
    }
}
