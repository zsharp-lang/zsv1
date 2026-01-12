namespace ZSharp.IR.VM
{
    public sealed class GetField(FieldReference field)
        : Instruction
        , IHasOperand<FieldReference>
    {
        public FieldReference Field { get; set; } = field;

        FieldReference IHasOperand<FieldReference>.Operand => Field;
    }
}
