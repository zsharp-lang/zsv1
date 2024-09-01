namespace ZSharp.IR.VM
{
    public abstract class JumpBase(Instruction target) 
        : Instruction
        , IHasOperand<Instruction>
    {
        public Instruction Target { get; set; } = target;

        Instruction IHasOperand<Instruction>.Operand => Target;
    }
}
