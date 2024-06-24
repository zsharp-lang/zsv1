namespace ZSharp.IR.VM
{
    public sealed class GetGlobal(Global global) : Instruction
    {
        public Global Global { get; } = global;
    }
}
