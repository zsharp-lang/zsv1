namespace ZSharp.SourceCompiler.Script
{
    public sealed partial class ScriptCompiler
    {
        public Interpreter.Interpreter Interpreter { get; }

        public AST.Document Node { get; }

        public string DocumentPath { get; }

        public ScriptCompiler(Interpreter.Interpreter interpreter, AST.Document document, string path)
        {
            Interpreter = interpreter;
            Node = document;

            CompileExpression = new TopLevelExpressionCompiler(interpreter)
            {
                CompileExpression = new ExpressionCompiler(interpreter)
                {
                    PostProcess = PostProcess
                }.Compile
            }.Compile;

            DocumentPath = path;
        }

        public void Compile()
        {
            //using var _ = Interpreter.Compiler.ContextScope(new DocumentContext());
            using var _ = Interpreter.Compiler.ContextScope(new ScopeContext());

            Node.Statements.ForEach(Compile);
        }
    }
}
