namespace ZSharp.IR.VM
{
    public abstract class JumpBase(Instruction target) : Instruction
    {
        public Instruction Target { get; set; } = target;
    }
}
