using CommonZ.Utils;

namespace ZSharp.IR
{
    public sealed class Enumclass : OOPType
    {
        public string? Name { get; set; }

        public EnumclassAttributes Attributes { get; set; } = EnumclassAttributes.None;

        public IType Type { get; set; }

        //public Collection<EnumValue> Values { get; }
    }
}
