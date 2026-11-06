namespace Package.DotNet
{
    public sealed class Interface(string name)
        : InterfaceReference
        , DotNetDefinition
    {
        public string Name { get; set; } = name;

        public List<InterfaceReference> Interfaces { get; } = [];

        public List<Property> Properties { get; } = [];

        public List<Method> Methods { get; } = [];
    }
}
