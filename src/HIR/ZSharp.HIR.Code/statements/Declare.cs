namespace ZSharp.HIR.Code
{
    /// <summary>
    /// Where a definition starts in code. What that means is up to the definition: a
    /// local takes its initial value here.
    /// </summary>
    public sealed class Declare(Definition definition)
        : Statement
    {
        public Definition Definition { get; set; } = definition;
    }
}
