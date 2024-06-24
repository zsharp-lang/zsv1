namespace ZSharp.IR
{
    public sealed class Property
    {
        public string Name { get; set; }

        public PropertyAttributes Attributes { get; set; } = PropertyAttributes.None;

        public IType Type { get; set; }

        public Function? Getter { get; set; }

        public Function? Setter { get; set; }

        public Property(string name, IType type)
        {
            Name = name;
            Type = type;
        }
    }
}
