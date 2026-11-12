namespace ZSharp.HIR.Code
{
    /// <summary>
    /// A pattern: what a value is matched against. It tests the value and binds parts of
    /// it. It isn't an expression: where a pattern is, a name is a place to bind, and
    /// <c>{ .x: 0 }</c> matches a value rather than making one.
    /// </summary>
    public interface Pattern
    {
    }
}
