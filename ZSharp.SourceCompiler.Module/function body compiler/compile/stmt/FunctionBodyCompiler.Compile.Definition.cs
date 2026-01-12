namespace ZSharp.SourceCompiler.Module
{
    partial class FunctionBodyCompiler
    {
        private IResult Compile(AST.DefinitionStatement definitionStatement)
            => Compile(definitionStatement.Definition);
    }
}
