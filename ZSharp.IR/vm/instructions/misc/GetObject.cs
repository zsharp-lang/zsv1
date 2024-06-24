namespace ZSharp.IR.VM
{
    public sealed class GetObject(IRObject ir) : Instruction
    {
        public IRObject IR { get; set; } = ir;
    }
}
