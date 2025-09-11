namespace ZSharp.SourceCompiler.Script
{
    public sealed partial class ScriptCompiler
    {
        public Interpreter.Interpreter Interpreter { get; }

        public AST.Document Node { get; }

        public ScriptCompiler(Interpreter.Interpreter interpreter, AST.Document document)
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
        }

        public void Compile()
        {
            //using var _ = Interpreter.Compiler.ContextScope(new DocumentContext());
            using var _ = Interpreter.Compiler.ContextScope(new ScopeContext());

            Node.Statements.ForEach(Compile);
        }
    }
}
