namespace ZSharp.SourceCompiler
{
    public sealed class TopLevelExpressionCompiler(Interpreter.Interpreter interpreter)
    {
        public Interpreter.Interpreter Interpreter { get; } = interpreter;

        public Func<AST.Expression, IResult> CompileExpression { get; init; } = new ExpressionCompiler(interpreter).Compile;

        public Func<object?, IResult> LoadCO { get; init; } = interpreter.RTLoader.Load;

        public IResult Compile(AST.Expression expression)
            => CompileExpression(expression);
    }
}
