namespace ZSharp.IR.VM
{
    public sealed class GetLocal(Local local) : Instruction
    {
        public Local Local { get; set; } = local;
    }
}
