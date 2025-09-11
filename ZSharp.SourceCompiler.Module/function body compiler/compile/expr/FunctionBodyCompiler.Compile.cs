namespace ZSharp.SourceCompiler.Module
{
    partial class FunctionBodyCompiler
    {
        private Result Compile(AST.Expression expression)
            => expression switch
            {
                AST.Function function => Compile(function),
                _ => CompileExpression(expression),
            };
    }
}
