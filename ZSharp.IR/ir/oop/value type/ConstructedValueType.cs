using CommonZ.Utils;

namespace ZSharp.IR
{
    public sealed class ConstructedValueType(ValueType @class)
        : ConstructedType<ValueType>
    {
        public ValueType ValueType { get; set; } = @class;

        ValueType OOPTypeReference<ValueType>.Definition => ValueType;

        public OOPTypeReference? OwningType { get; set; }

        public Collection<IType> Arguments { get; set; } = false ? [] : Collection<IType>.Empty;
    }
}
