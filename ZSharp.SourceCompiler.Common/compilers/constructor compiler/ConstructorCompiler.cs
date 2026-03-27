namespace ZSharp.SourceCompiler
{
    public sealed partial class ConstructorCompiler
    {
        private Interpreter.Interpreter Interpreter { get; }

        private AST.Constructor Node { get; }

        private Objects.Constructor Object { get; }

        private CompileExpression CompileExpression { get; }

        public ConstructorCompiler(Interpreter.Interpreter interpreter, AST.Constructor constructor)
        {
            Interpreter = interpreter;
            Node = constructor;
            Object = new()
            {
                Name = constructor.Name,
            };

            CompileExpression = new ExpressionCompiler(interpreter).Compile;
        }
    }
}
