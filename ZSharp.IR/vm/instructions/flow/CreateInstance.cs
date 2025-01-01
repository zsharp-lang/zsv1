namespace ZSharp.IR.VM
{
    public sealed class CreateInstance(Constructor constructor) 
        : Instruction
        , IHasOperand<Constructor>
    {
        public Constructor Constructor { get; set; } = constructor;

        Constructor IHasOperand<Constructor>.Operand => Constructor;
    }
}
