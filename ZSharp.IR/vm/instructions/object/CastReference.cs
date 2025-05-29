namespace ZSharp.IR.VM
{
    public sealed class CastReference(OOPTypeReference targetType) : Instruction
    {
        public OOPTypeReference Type { get; set; } = targetType;
    }
}
