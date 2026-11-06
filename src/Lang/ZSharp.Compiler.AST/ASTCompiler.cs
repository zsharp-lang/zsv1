global using CommonZ;

namespace ZSharp.Compiler
{
    public delegate IResult<HIR.Code.Expression, Error> CompileASTExpression(AST.Expression expression);

    public delegate IResult<HIR.Code.Statement, Error> CompileASTStatement(AST.Statement statement);

    public delegate IResult<CompiledDefinition, Error> CompileASTDefinition(AST.Definition definition);

    public class ASTCompiler
    {
        public CompileASTExpression CompileExpression { get; set; } = expression
            => Result<HIR.Code.Expression>.Error(Unsupported(expression));

        public CompileASTStatement CompileStatement { get; set; } = statement
            => Result<HIR.Code.Statement>.Error(Unsupported(statement));

        public CompileASTDefinition CompileDefinition { get; set; } = definition
            => Result<CompiledDefinition>.Error(Unsupported(definition));

        public ASTCompiler Clone() => (ASTCompiler)MemberwiseClone();

        private static UnsupportedError Unsupported(AST.Node node)
            => new($"Can't compile AST node of type {node.GetType().Name}.");
    }
}
