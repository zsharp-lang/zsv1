
namespace ZSharp.ZSSourceCompiler
{
    public sealed partial class DocumentCompiler(ZSSourceCompiler compiler, AST.Document node, Document document)
        : ContextCompiler<AST.Document, Document>(compiler, node, document)
        , IOverrideCompileExpression
        , IOverrideCompileStatement
    {
        public override Document Compile()
        {
            using (Context.Compiler(this))
            using (Context.Scope(Object))
                CompileDocument();

            return base.Compile();
        }

        private void CompileDocument()
        {
            foreach (var item in Node.Statements)
                Compiler.CompileNode(item);
        }

        public CompilerObject? CompileNode(ZSSourceCompiler compiler, Statement statement)
        {
            //if (statement is ExpressionStatement expressionStatement)
            //    return Compiler.Compiler.Evaluate(Compiler.CompileNode(expressionStatement.Expression));

            return null;
            // if the statement is a definition, compile it
        }

        public CompilerObject? CompileNode(ZSSourceCompiler compiler, Expression node)
            => node switch
            {
                Module module => Compile(module),
                _ => null
            };
    }
}
