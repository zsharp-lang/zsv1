namespace ZSharp.LIR
{
    public sealed class GetLocal(Variable variable)
        : Expression
    {
        public Variable Variable { get; set; } = variable;
    }
}
