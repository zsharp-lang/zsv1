namespace ZSharp.SourceCompiler
{
    public sealed class TopLevelExpressionCompiler(Interpreter.Interpreter interpreter)
    {
        public Interpreter.Interpreter Interpreter { get; } = interpreter;

        public Func<AST.Expression, Result> CompileExpression { get; init; } = new ExpressionCompiler(interpreter).Compile;

        public Func<object?, Result> LoadCO { get; init; } = interpreter.RTLoader.Load;

        public Result Compile(AST.Expression expression)
            => CompileExpression(expression);
    }
}
