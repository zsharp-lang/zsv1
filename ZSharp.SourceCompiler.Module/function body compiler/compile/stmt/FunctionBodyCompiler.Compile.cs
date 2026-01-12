namespace ZSharp.SourceCompiler.Module
{
    partial class FunctionBodyCompiler
    {
        private IResult Compile(AST.Statement statement)
            => statement switch
            {
                AST.BlockStatement block => Compile(block),
                AST.DefinitionStatement definition => Compile(definition),
                AST.ExpressionStatement expression => Compile(expression),
                AST.Return @return => Compile(@return),
                _ => Result.Error($"Unknown statement type: {statement.GetType().Name}"),
            };
    }
}
