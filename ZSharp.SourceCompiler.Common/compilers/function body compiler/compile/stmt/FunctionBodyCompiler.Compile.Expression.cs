namespace ZSharp.SourceCompiler
{
    partial class FunctionBodyCompiler
    {
        private IResult Compile(AST.ExpressionStatement expressionStatement)
            => CompileExpression(expressionStatement.Expression);
    }
}
