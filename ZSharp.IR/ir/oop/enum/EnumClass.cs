using CommonZ.Utils;

namespace ZSharp.IR
{
    public sealed class EnumClass
        : OOPType
        , IType
    {
        public string? Name { get; set; }

        public EnumclassAttributes Attributes { get; set; } = EnumclassAttributes.None;

        public IType Type { get; set; } = null!;

        public Collection<EnumValue> Values { get; }

        public EnumClass(string? name)
        {
            Name = name;
            Values = new EnumValueCollection(this);
        }

        public EnumClass(string? name, IType type)
            : this(name)
        {
            Type = type;
        }
    }
}
