namespace Package.DotNet
{
    public sealed class Property(string name, ZSharp.HIR.Type type)
        : ZSharp.HIR.Definition
        , ZSharp.HIR.Instance
    {
        public string Name { get; set; } = name;

        public ZSharp.HIR.Type Type { get; set; } = type;

        public bool IsStatic { get; set; }

        public Method? Getter { get; set; }

        public Method? Setter { get; set; }
    }
}
