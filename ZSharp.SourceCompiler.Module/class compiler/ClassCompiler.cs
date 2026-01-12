namespace ZSharp.SourceCompiler.Module
{
    public sealed partial class ClassCompiler
    {
        private Objects.Module Object { get; }

        private Interpreter.Interpreter Interpreter { get; }

        private AST.Module Node { get; }

        private CompileExpression CompileExpression { get; }

        public ClassCompiler(Interpreter.Interpreter interpreter, AST.Module module)
        {
            Interpreter = interpreter;
            Node = module;
            Object = new();

            CompileExpression = new TopLevelExpressionCompiler(interpreter).Compile;

            tasks = new(InitCompile);
        }
    }
}
