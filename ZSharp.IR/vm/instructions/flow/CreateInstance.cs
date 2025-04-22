namespace ZSharp.IR.VM
{
    public sealed class CreateInstance(ConstructorReference constructor) 
        : Instruction
        , IHasOperand<ConstructorReference>
    {
        public ConstructorReference Constructor { get; set; } = constructor;

        ConstructorReference IHasOperand<ConstructorReference>.Operand => Constructor;
    }
}
