namespace ZSharp.SourceCompiler.Module
{
    internal sealed partial class FunctionBodyCompiler
    {
        private Interpreter.Interpreter Interpreter { get; }

        private AST.Statement Node { get; }

        private CompileExpression CompileExpression { get; }

        public FunctionBodyCompiler(Interpreter.Interpreter interpreter, AST.Statement body)
        {
            Interpreter = interpreter;
            Node = body;

            CompileExpression = new ExpressionCompiler(interpreter).Compile;
        }
    }
}
