namespace ZSharp.SourceCompiler.Module
{
    public sealed partial class ClassCompiler
    {
        private Objects.Module Object { get; }

        private Interpreter.Interpreter Interpreter { get; }

        private AST.TypeDefinition Node { get; }

        private CompileExpression CompileExpression { get; }

        public ClassCompiler(Interpreter.Interpreter interpreter, AST.TypeDefinition definition)
        {
            Interpreter = interpreter;
            Node = definition;
            Object = new();

            CompileExpression = new TopLevelExpressionCompiler(interpreter).Compile;

            tasks = new(InitCompile);
        }
    }
}
