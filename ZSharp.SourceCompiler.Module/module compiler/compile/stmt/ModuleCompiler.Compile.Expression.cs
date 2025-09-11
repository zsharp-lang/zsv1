namespace ZSharp.SourceCompiler.Module
{
    partial class ModuleCompiler
    {
        private Result Compile(AST.ExpressionStatement expressionStatement)
            => CompileExpression(expressionStatement.Expression);
    }
}
