namespace ZSharp.HIR.Code
{
    /// <summary>
    /// A name to bind, as <c>let circle: Circle</c> is: it matches what its type matches,
    /// or anything if it has none, and sets its target to the value, as its type has it.
    /// The target is what is set, which its declaration makes, where the pattern is; the
    /// pattern refers to it, and isn't it.
    /// </summary>
    public sealed class NamePattern
        : Pattern
    {
        public required Expression Target { get; set; }

        public Expression? Type { get; set; }
    }
}
