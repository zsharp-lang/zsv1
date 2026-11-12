namespace Package.DotNet
{
    /// <summary>
    /// A module: the root of what it holds, defined in a document, so it is an instance
    /// and not a definition another one holds.
    /// </summary>
    public sealed class Module(string name)
        : ZSharp.HIR.Instance
        , ZSharp.HIR.Named
        , Accessible
    {
        public Accessibility Accessibility { get; set; } = Accessibility.Public;

        public string Name { get; set; } = name;

        public List<DotNetDefinition> Definitions { get; } = [];

        public List<Field> Fields { get; } = [];

        public List<Property> Properties { get; } = [];

        public List<Method> Methods { get; } = [];
    }
}
