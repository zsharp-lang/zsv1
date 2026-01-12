namespace ZSharp.SourceCompiler.Module
{
    partial class ClassCompiler
    {
        private IResult Compile(AST.ExpressionStatement expressionStatement)
            => CompileExpression(expressionStatement.Expression);
    }
}
