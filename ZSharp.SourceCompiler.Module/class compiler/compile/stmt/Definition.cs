namespace ZSharp.SourceCompiler.Module
{
    partial class ClassCompiler
    {
        private IResult Compile(AST.DefinitionStatement definitionStatement)
            => definitionStatement.Definition switch
            {
                AST.Function function => Compile(function),
                _ => Result.Error($"Unknown definition type: {definitionStatement.Definition.GetType().Name}")
            };
    }
}
