namespace ZSharp.IR.VM
{
    public sealed class GetClass(OOPTypeReference<Class> @class)
        : Instruction
        , IHasOperand<OOPTypeReference<Class>>
    {
        public OOPTypeReference<Class> Class { get; set; } = @class;

        OOPTypeReference<Class> IHasOperand<OOPTypeReference<Class>>.Operand => Class;
    }
}
