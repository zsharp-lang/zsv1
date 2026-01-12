namespace ZSharp.IR.VM
{
    public sealed class GetClass(TypeReference<Class> @class)
        : Instruction
        , IHasOperand<TypeReference<Class>>
    {
        public TypeReference<Class> Class { get; set; } = @class;

        TypeReference<Class> IHasOperand<TypeReference<Class>>.Operand => Class;
    }
}
