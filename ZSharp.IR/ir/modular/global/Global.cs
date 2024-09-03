namespace ZSharp.IR
{
    public sealed class Global(string name, IType type) : ModuleMember
    {
        public string Name { get; set; } = name;

        public GlobalAttributes Attributes { get; set; } = GlobalAttributes.None;

        public IType Type { get; set; } = type;

        public VM.Instruction[]? Initializer { get; set; }

        public int Index { get; internal set; }
    }
}
