namespace ZSharp.IR.VM
{
    public sealed class SetField(FieldReference field)
        : Instruction
        , IHasOperand<FieldReference>
    {
        public FieldReference Field { get; set; } = field;

        FieldReference IHasOperand<FieldReference>.Operand => Field;
    }
}
