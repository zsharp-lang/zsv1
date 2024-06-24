namespace ZSharp.IR
{
    public sealed class TypeAlias
    {
        public string? Name { get; set; }

        public TypeAliasAttributes Attributes { get; set; } = TypeAliasAttributes.None;

        public Signature Parameter
        {
            get => Function.Signature;
            set => Function.Signature = value;
        }

        public Function Function { get; set; }
    }
}
