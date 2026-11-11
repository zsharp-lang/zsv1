namespace ZSharp.AST
{
    /// <summary>
    /// A statement with a decorator, as in <c>@decorator class Foo {}</c>.
    /// </summary>
    public sealed class DecoratedStatement : Statement
    {
        public required Expression Decorator { get; init; }

        public required Statement Statement { get; init; }

        public override string ToString()
            => $"@{Decorator} {Statement}";
    }
}
