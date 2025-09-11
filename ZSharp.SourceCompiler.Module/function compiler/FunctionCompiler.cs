namespace ZSharp.SourceCompiler.Module
{
    internal sealed partial class FunctionCompiler
    {
        private Interpreter.Interpreter Interpreter { get; }

        private AST.Function Node { get; }

        private Objects.Function Object { get; }

        private CompileExpression CompileExpression { get; }

        public FunctionCompiler(Interpreter.Interpreter interpreter, AST.Function function)
        {
            Interpreter = interpreter;
            Node = function;
            Object = new()
            {
                Name = function.Name,
            };

            CompileExpression = new ExpressionCompiler(interpreter).Compile;
        }
    }
}
