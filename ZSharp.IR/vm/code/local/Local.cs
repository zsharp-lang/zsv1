namespace ZSharp.IR.VM
{
    public sealed class Local(string name, IType type) : IRObject
    {
        public override Module? Module => Owner?.Module;

        public IRObject? Owner { get; internal set; }

        public LocalAttributes Attributes { get; set; }

        public string Name { get; set; } = name;

        public IType Type { get; set; } = type;

        public int Index { get; internal set; }

        public bool IsReadOnly
        {
            get => (Attributes & LocalAttributes.ReadOnly) == LocalAttributes.ReadOnly;
            set => Attributes = value ? Attributes | LocalAttributes.ReadOnly : Attributes & ~LocalAttributes.ReadOnly;
        }

        public Instruction[]? Initializer { get; set; }
    }
}
