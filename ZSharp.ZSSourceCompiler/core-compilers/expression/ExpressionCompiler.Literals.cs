namespace ZSharp.ZSSourceCompiler
{
    public sealed partial class ExpressionCompiler
    {
        public Objects.ArrayLiteral Compile(ArrayLiteral array)
            => new(array.Items.Select(Compiler.CompileNode));

        public CompilerObject Compile(LiteralExpression literal)
            => literal.Type switch
            {
                LiteralType.String => Compiler.Compiler.CreateString(literal.Value),
                LiteralType.False => Compiler.Compiler.CreateFalse(),
                LiteralType.True => Compiler.Compiler.CreateTrue(),
                LiteralType.Null => Compiler.Compiler.CreateNull(),
                LiteralType.Number => Compiler.Compiler.CreateInteger(int.Parse(literal.Value)),
                _ => throw new($"Could not find suitable compiler for node of type {typeof(LiteralExpression).Name}"), // TODO: proper exception: unknown literal type
            };
    }
}
