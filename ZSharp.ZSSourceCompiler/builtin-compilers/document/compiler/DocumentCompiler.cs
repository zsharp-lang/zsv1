
using ZSharp.AST;

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
            using (Compiler.Compiler.ContextScope(new ObjectContext<Document>(Object)))
            using (Context.Scope(Object))
                CompileDocument();

            return base.Compile();
        }

        private void CompileDocument()
        {
            foreach (var item in Node.Statements)
                Compiler.CompileNode(item);
        }

        public ObjectResult? CompileNode(ZSSourceCompiler compiler, Statement statement)
        {
            if (statement is ExpressionStatement expressionStatement)
                return expressionStatement.Expression switch
                {
                    Module => null,
                    Expression expression => ObjectResult.Ok(
                        Compiler.Compiler.Feature<Compiler.IRCompiler>().EvaluateCO(
                            Compiler.Compiler.IR.CompileCode(
                                Compiler.CompileNode(expression).Unwrap()
                            ).Unwrap()
                        ) ?? new Objects.RawCode(new())
                    ),
                };

            return null;
            // if the statement is a definition, compile it
        }

        public ObjectResult? CompileNode(ZSSourceCompiler compiler, Expression node)
            => node switch
            {
                Module module => Compile(module),
                _ => null,
            } is CompilerObject result
            ? ObjectResult.Ok(result)
            : null;
    }
}
