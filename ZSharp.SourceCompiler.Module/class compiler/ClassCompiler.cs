namespace ZSharp.SourceCompiler.Module
{
    public sealed partial class ClassCompiler
    {
        private Objects.Module Object { get; }

        private Interpreter.Interpreter Interpreter { get; }

        private AST.OOPDefinition Node { get; }

        private CompileExpression CompileExpression { get; }

        public ClassCompiler(Interpreter.Interpreter interpreter, AST.OOPDefinition definition)
        {
            Interpreter = interpreter;
            Node = definition;
            Object = new();

            CompileExpression = new TopLevelExpressionCompiler(interpreter).Compile;

            tasks = new(InitCompile);
        }
    }
}
