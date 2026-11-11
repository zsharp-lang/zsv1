namespace ZSharp.LIR
{
    public sealed class SetLocal(Variable variable, Expression value)
        : Statement
    {
        public Variable Variable { get; set; } = variable;

        public Expression Value { get; set; } = value;
    }
}
