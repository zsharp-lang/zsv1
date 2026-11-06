namespace ZSharp.HIR.Code
{
    /// <summary>
    /// Code that runs with whatever builds the code around it, and produces an
    /// instance for that code to use. This is how a definition that isn't cast gets
    /// its identity: reifying the code around it leaves this inner code unreified.
    /// </summary>
    public sealed class Escape(Expression value)
        : Expression
    {
        public Expression Value { get; set; } = value;
    }
}
