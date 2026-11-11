namespace Package.DotNet
{
    /// <summary>
    /// How C# lifts an operator to nullable operands.
    /// </summary>
    public enum Lifting
    {
        /// <summary><c>null</c> if an operand is, as <c>+</c> is.</summary>
        Value,

        /// <summary>Equal if both operands are <c>null</c>, as <c>==</c> and <c>!=</c> are.</summary>
        Equality,

        /// <summary><c>false</c> if an operand is <c>null</c>, as <c>&lt;</c> is.</summary>
        Comparison,
    }

    /// <summary>
    /// An operator lifted to nullable operands, as C# lifts every operator: applied to
    /// their values when they all have one.
    /// </summary>
    public sealed class LiftedOperator(ZSharp.HIR.Code.OverloadGroup @operator, Lifting lifting)
        : ZSharp.HIR.Instance
    {
        /// <summary>
        /// The operator lifted, which the lifted one is an overload of.
        /// </summary>
        public ZSharp.HIR.Code.OverloadGroup Operator { get; set; } = @operator;

        public Lifting Lifting { get; set; } = lifting;
    }
}
