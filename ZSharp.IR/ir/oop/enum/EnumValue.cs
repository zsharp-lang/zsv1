using CommonZ.Utils;

namespace ZSharp.IR
{
    public sealed class EnumValue(string name, Collection<VM.Instruction> valueCode)
        : ModuleMember
    {
        internal EnumClass? _enumClass;

        public string Name { get; set; } = name;

        public Collection<VM.Instruction> Value { get; set; } = valueCode;
    }
}
