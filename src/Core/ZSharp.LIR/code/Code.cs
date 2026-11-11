namespace ZSharp.LIR
{
    /// <summary>
    /// The body of a function.
    /// </summary>
    public sealed class Code
    {
        public Block Block { get; } = new();
    }
}
