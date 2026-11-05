namespace ZSharp.LIR
{
    public sealed class GetGlobal(Global global)
        : Expression
    {
        public Global Global { get; set; } = global;
    }
}
