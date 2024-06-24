namespace ZSharp.IR.VM
{
    public sealed class SetGlobal(Global global) : Instruction
    {
        public Global Global { get; } = global;
    }
}
