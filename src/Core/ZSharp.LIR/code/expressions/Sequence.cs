namespace ZSharp.LIR
{
    /// <summary>
    /// Runs statements, then produces a value. It lets a lowerer use temporaries
    /// where an expression is expected.
    /// </summary>
    public sealed class Sequence(Block block, Expression result)
        : Expression
    {
        public Block Block { get; set; } = block;

        public Expression Result { get; set; } = result;
    }
}
