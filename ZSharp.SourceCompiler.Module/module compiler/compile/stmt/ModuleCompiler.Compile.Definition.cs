namespace ZSharp.SourceCompiler.Module
{
    partial class ModuleCompiler
    {
        private IResult Compile(AST.DefinitionStatement definitionStatement)
            => definitionStatement.Definition switch
            {
                AST.Function function => Compile(function),
                AST.OOPDefinition oopDefinition => Compile(oopDefinition),
                _ => Result.Error($"Unknown definition type: {definitionStatement.Definition.GetType().Name}")
            };
    }
}
