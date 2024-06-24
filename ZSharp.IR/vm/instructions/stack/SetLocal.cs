namespace ZSharp.IR.VM
{
    public sealed class SetLocal(Local local) : Instruction
    {
        public Local Local { get; set; } = local;
    }
}
