using CommonZ.Utils;

namespace ZSharp.IR
{
    public sealed class ConstructedValueType(ValueType @class)
        : ConstructedType<ValueType>
    {
        public ValueType ValueType { get; set; } = @class;

        ValueType TypeReference<ValueType>.Definition => ValueType;

        public TypeReference? OwningType { get; set; }

        public Collection<IType> Arguments { get; set; } = false ? [] : Collection<IType>.Empty;
    }
}
