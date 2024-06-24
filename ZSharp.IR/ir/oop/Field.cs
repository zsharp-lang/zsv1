namespace ZSharp.IR
{
    public sealed class Field
    {
        public string Name { get; set; }

        public FieldAttributes Attributes { get; set; } = FieldAttributes.None;

        public IType Type { get; set; }

        public VM.Instruction[]? Initializer { get; set; }
    }
}
