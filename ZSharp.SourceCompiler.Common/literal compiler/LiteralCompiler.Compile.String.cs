namespace ZSharp.SourceCompiler
{
    partial class LiteralCompiler
    {
        private Result CompileString(AST.LiteralExpression expression)
        {
            return Result.Ok(new StringLiteral(expression.Value));
        }
    }
}
