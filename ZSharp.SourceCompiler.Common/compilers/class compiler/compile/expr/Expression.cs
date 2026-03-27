namespace ZSharp.SourceCompiler.Class
{
    partial class ClassCompiler
    {
        private IResult Compile(AST.Expression expression)
            => expression switch
            {
                AST.CallExpression call => Compile(call),
                AST.Function function => Compile(function),
                AST.LetExpression let => Compile(let),
                AST.VarExpression var => Compile(var),
                _ => CompileExpression(expression)
            };
    }
}
