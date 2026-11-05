namespace ZSharp.LIR
{
    /// <summary>
    /// The body of a function.
    /// </summary>
    public sealed class Code
    {
        public List<Local> Locals { get; } = [];

        public Block Block { get; } = new();
    }
}
