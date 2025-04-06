namespace ZSharp.IR.VM
{
    public sealed class CreateInstance(MemberReference<Constructor> constructor) 
        : Instruction
        , IHasOperand<MemberReference<Constructor>>
    {
        public MemberReference<Constructor> Constructor { get; set; } = constructor;

        MemberReference<Constructor> IHasOperand<MemberReference<Constructor>>.Operand => Constructor;
    }
}
