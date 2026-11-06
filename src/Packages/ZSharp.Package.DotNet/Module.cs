namespace Package.DotNet
{
    public sealed class Module(string name)
        : ZSharp.HIR.Definition
        , ZSharp.HIR.Instance
    {
        public string Name { get; set; } = name;

        public List<DotNetDefinition> Definitions { get; } = [];

        public List<Field> Fields { get; } = [];

        public List<Property> Properties { get; } = [];

        public List<Method> Methods { get; } = [];
    }
}
