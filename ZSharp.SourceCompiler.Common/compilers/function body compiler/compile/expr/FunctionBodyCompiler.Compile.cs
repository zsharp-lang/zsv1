namespace ZSharp.SourceCompiler
{
    partial class FunctionBodyCompiler
    {
        private IResult Compile(AST.Expression expression)
            => expression switch
            {
                AST.Function function => Compile(function),
                _ => CompileExpression(expression),
            };
    }
}
