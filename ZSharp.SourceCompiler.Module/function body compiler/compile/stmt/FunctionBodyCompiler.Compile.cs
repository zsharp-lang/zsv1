namespace ZSharp.SourceCompiler.Module
{
    partial class FunctionBodyCompiler
    {
        private Result Compile(AST.Statement statement)
            => statement switch
            {
                AST.BlockStatement block => Compile(block),
                AST.ExpressionStatement expression => Compile(expression),
                AST.DefinitionStatement definition => Compile(definition),
                _ => Result.Error($"Unknown statement type: {statement.GetType().Name}"),
            };
    }
}
