namespace Package.DotNet
{
    public sealed class Struct(string name)
        : StructReference
        , DotNetDefinition
        , ZSharp.HIR.Named
        , ZSharp.HIR.Annotated
    {
        public List<object> Annotations { get; } = [];

        public string Name { get; set; } = name;

        public List<InterfaceReference> Interfaces { get; } = [];

        public List<Constructor> Constructors { get; } = [];

        public StaticConstructor? StaticConstructor { get; set; }

        public List<Field> Fields { get; } = [];

        public List<Property> Properties { get; } = [];

        public List<Method> Methods { get; } = [];

        public List<DotNetDefinition> NestedDefinitions { get; } = [];
    }
}
