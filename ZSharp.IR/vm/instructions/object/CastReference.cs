namespace ZSharp.IR.VM
{
    public sealed class CastReference(TypeReference targetType) : Instruction
    {
        public TypeReference Type { get; set; } = targetType;
    }
}
