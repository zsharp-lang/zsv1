namespace ZSharp.AST
{
    public sealed class ArrayLiteral : Expression
    {
        public List<Expression> Items { get; set; } = [];
    }
}
