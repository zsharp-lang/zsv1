namespace ZSharp.SourceCompiler.Class
{
    partial class ClassCompiler
    {
        private IResult Compile(AST.Expression expression)
            => expression switch
            {
                AST.CallExpression call => Compile(call),
                AST.Function function => Compile(function),
                _ => CompileExpression(expression)
            };
    }
}
