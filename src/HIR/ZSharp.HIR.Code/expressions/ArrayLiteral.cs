namespace ZSharp.HIR.Code
{
    /// <summary>
    /// Values written as one, as in <c>[1, 2, 3]</c>. It has no type of its own: what it
    /// goes to says what collection it is, as with a collection expression in C#.
    /// </summary>
    public sealed class ArrayLiteral
        : Expression
    {
        public List<Expression> Items { get; init; } = [];
    }
}
