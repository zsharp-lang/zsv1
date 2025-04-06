namespace ZSharp.IR.VM
{
    public sealed class GetField(MemberReference<Field> field)
        : Instruction
        , IHasOperand<MemberReference<Field>>
    {
        public MemberReference<Field> Field { get; set; } = field;

        MemberReference<Field> IHasOperand<MemberReference<Field>>.Operand => Field;
    }
}
