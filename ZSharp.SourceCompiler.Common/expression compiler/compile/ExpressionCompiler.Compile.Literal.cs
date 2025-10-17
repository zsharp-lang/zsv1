namespace ZSharp.SourceCompiler
{
    partial class ExpressionCompiler
    {
        private Result Compile(AST.LiteralExpression literal)
        {
            if (literal.UnitType is not null)
                Interpreter.Log.Warning($"Literal has a unit type '{literal.UnitType}' which will be ignored.", new NodeLogOrigin(literal));

            return new LiteralCompiler(Interpreter).Compile(literal);
        }
    }
}
