namespace ZSharp.IR.VM
{
    public sealed class SetField(Field field)
        : Instruction
        , IHasOperand<Field>
    {
        public Field Field { get; set; } = field;

        Field IHasOperand<Field>.Operand => Field;
    }
}
