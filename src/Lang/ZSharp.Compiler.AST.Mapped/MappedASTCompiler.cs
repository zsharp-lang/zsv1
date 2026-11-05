namespace ZSharp.Compiler
{
    /// <summary>
    /// Implements <see cref="ASTCompiler"/> through registered compilers.
    /// </summary>
    public sealed class MappedASTCompiler(ASTCompiler compiler)
    {
        private readonly ASTCompiler compiler = compiler;

        private ASTCompiler @base = null!;

        public ASTMap<AST.Expression, HIR.Code.Expression> Expressions { get; } = new();

        public ASTMap<AST.Statement, HIR.Code.Statement> Statements { get; } = new();

        public void Apply()
        {
            @base = compiler.Clone();

            compiler.CompileExpression = CompileExpression;
            compiler.CompileStatement = CompileStatement;
        }

        private IResult<HIR.Code.Expression, Error> CompileExpression(AST.Expression expression)
        {
            var result = @base.CompileExpression(expression);

            if (result.IsUnsupported())
                result = Expressions.Compile(expression);

            return result;
        }

        private IResult<HIR.Code.Statement, Error> CompileStatement(AST.Statement statement)
        {
            var result = @base.CompileStatement(statement);

            if (result.IsUnsupported())
                result = Statements.Compile(statement);

            return result;
        }
    }
}
