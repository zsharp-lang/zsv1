namespace ZSharp.SourceCompiler.Class
{
    public sealed partial class ClassCompiler
    {
        private Objects.ClassDefinitionProxy Object { get; }

        private Objects.ClassSpecification Spec { get; }

        private Interpreter.Interpreter Interpreter { get; }

        private AST.TypeDefinition Node { get; }

        private CompileExpression CompileExpression { get; }

        public ClassCompiler(Interpreter.Interpreter interpreter, AST.TypeDefinition definition)
        {
            Interpreter = interpreter;
            Node = definition;
            Object = new();

            Spec = new()
            {
                Name = definition.Name,
                Definition = Object,
                Bases = [],
                Content = [],
                Members = []
            };

            CompileExpression = new TopLevelExpressionCompiler(interpreter).Compile;

            tasks = new(InitCompile);
        }

        public CompilerObject GetObject() => Object;

        public Objects.ClassSpecification GetSpecification() => Spec;
    }
}
