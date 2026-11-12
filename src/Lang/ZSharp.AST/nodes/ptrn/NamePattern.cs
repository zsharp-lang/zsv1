namespace ZSharp.AST
{
    /// <summary>
    /// The keyword a name pattern is written with: <c>let name</c> or <c>var name</c>. What
    /// it means, as whether the name can be set again, is the language's.
    /// </summary>
    public enum NameKeyword
    {
        Let,
        Var,
    }

    /// <summary>
    /// A name to bind, written with its keyword, as <c>let circle: Circle</c>, with the type
    /// it matches if it says one.
    /// </summary>
    public sealed class NamePattern : Pattern
    {
        public required NameKeyword Keyword { get; set; }

        public required string Name { get; set; }

        public Expression? Type { get; set; }
    }
}
