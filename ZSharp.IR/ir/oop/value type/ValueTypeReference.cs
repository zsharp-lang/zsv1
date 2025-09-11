namespace ZSharp.IR
{
    public sealed class ValueTypeReference(ValueType valueType)
        : OOPTypeReference<ValueType>
    {
        public ValueType Definition { get; } = valueType;

        public OOPTypeReference? OwningType { get; set; }
    }
}
