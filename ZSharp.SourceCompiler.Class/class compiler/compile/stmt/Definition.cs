namespace ZSharp.SourceCompiler.Class
{
    partial class ClassCompiler
    {
        private IResult Compile(AST.DefinitionStatement definitionStatement)
            => definitionStatement.Definition switch
            {
                AST.Function function => Compile(function),
                AST.LetExpression let => Compile(let),
                AST.VarExpression var => Compile(var),
                _ => Result.Error($"Unknown definition type: {definitionStatement.Definition.GetType().Name}")
            };
    }
}
