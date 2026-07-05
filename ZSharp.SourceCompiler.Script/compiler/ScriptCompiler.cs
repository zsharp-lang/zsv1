namespace ZSharp.SourceCompiler.Script
{
    public sealed partial class ScriptCompiler
    {
        public required Interpreter.Interpreter Interpreter { get; init; }

        public AST.Document Document { get; }

        public string DocumentPath { get; }

        public ScriptCompiler(AST.Document document, string path)
        {
            Document = document;

            //CompileExpression = new TopLevelExpressionCompiler(interpreter)
            //{
            //    CompileExpression = new ExpressionCompiler(interpreter)
            //    {
            //        PostProcess = PostProcess
            //    }.Compile
            //}.Compile;

            DocumentPath = path;
        }

        public void Compile()
        {
            //using var _ = Interpreter.Compiler.ContextScope(new DocumentContext());
            using var _ = Interpreter.Compiler.ContextScope(new ScopeContext());

            Document.Statements.ForEach(Compile);
        }
    }
}
