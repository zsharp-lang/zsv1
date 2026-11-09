namespace Package.DotNet
{
    public sealed class Interface(string name)
        : InterfaceReference
        , DotNetDefinition
        , ZSharp.HIR.Named
    {
        public string Name { get; set; } = name;

        /// <summary>
        /// The interfaces it extends, which may be generic instances of interfaces.
        /// </summary>
        public List<TypeReference> Interfaces { get; } = [];

        public List<Property> Properties { get; } = [];

        public List<Method> Methods { get; } = [];
    }
}
