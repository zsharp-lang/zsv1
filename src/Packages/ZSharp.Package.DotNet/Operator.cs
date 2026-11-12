namespace Package.DotNet
{
    /// <summary>
    /// The operators the CLR types of the operands define, by the name C# gives them, as
    /// <c>op_Addition</c> for <c>+</c>.
    /// </summary>
    public sealed class Operator(string name)
        : ZSharp.HIR.Instance
    {
        public string Name { get; set; } = name;
    }
}
