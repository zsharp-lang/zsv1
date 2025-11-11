namespace ZSharp.SourceCompiler
{
    partial class FunctionBodyCompiler
    {
        private IResult Compile(AST.DefinitionStatement definitionStatement)
            => Compile(definitionStatement.Definition);
    }
}
