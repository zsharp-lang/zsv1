namespace ZSharp.SourceCompiler.Module
{
    partial class ClassCompiler
    {
        private Result Compile(AST.ExpressionStatement expressionStatement)
            => CompileExpression(expressionStatement.Expression);
    }
}
