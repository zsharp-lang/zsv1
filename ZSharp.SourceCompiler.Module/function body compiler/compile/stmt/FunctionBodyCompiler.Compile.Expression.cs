namespace ZSharp.SourceCompiler.Module
{
    partial class FunctionBodyCompiler
    {
        private Result Compile(AST.ExpressionStatement expressionStatement)
            => CompileExpression(expressionStatement.Expression);
    }
}
