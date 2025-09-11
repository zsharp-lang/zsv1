namespace ZSharp.SourceCompiler.Module
{
    partial class FunctionBodyCompiler
    {
        private Result Compile(AST.DefinitionStatement definitionStatement)
            => Compile(definitionStatement.Definition);
    }
}
