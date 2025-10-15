namespace ZSharp.IR
{
    public sealed class ValueTypeReference(ValueType valueType)
        : TypeReference<ValueType>
    {
        public ValueType Definition { get; } = valueType;

        public TypeReference? OwningType { get; set; }
    }
}
