namespace ZSharp.AST
{
    /// <summary>
    /// A name to bind: <c>let name</c>, or <c>var name</c> for one that can be set again,
    /// with the type it matches, as in <c>let circle: Circle</c>, if it says one.
    /// </summary>
    public sealed class NamePattern : Pattern
    {
        public required bool IsMutable { get; set; }

        public required string Name { get; set; }

        public Expression? Type { get; set; }
    }
}
