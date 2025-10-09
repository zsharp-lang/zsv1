namespace ZSharp.SourceCompiler.Module
{
    partial class ClassCompiler
    {
        private Result Compile(AST.Expression expression)
            => expression switch
            {
                AST.Function function => Compile(function),
                _ => CompileExpression(expression)
            };
    }
}
