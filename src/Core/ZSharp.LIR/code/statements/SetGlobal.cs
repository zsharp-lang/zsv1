namespace ZSharp.LIR
{
    public sealed class SetGlobal(Global global, Expression value)
        : Statement
    {
        public Global Global { get; set; } = global;

        public Expression Value { get; set; } = value;
    }
}
