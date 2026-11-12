namespace Package.DotNet
{
    /// <summary>
    /// A class that already exists is identity only, since HIR is never lazy and
    /// reflecting its members would reach much of the BCL: its members are found
    /// by the CO it loads as. A class being defined lists them here. The same goes
    /// for every other definition in this package.
    /// </summary>
    public sealed class Class(string name)
        : ClassReference
        , DotNetDefinition
        , ZSharp.HIR.Named
        , ZSharp.HIR.Annotated
        , Accessible
    {
        public Accessibility Accessibility { get; set; } = Accessibility.Public;

        public List<object> Annotations { get; } = [];

        public string Name { get; set; } = name;

        public bool IsAbstract { get; set; }

        public bool IsSealed { get; set; }

        public bool IsStatic { get; set; }

        public ClassReference? Base { get; set; }

        /// <summary>
        /// The interfaces it implements, which may be generic instances of interfaces.
        /// </summary>
        public List<TypeReference> Interfaces { get; } = [];

        /// <summary>
        /// The type parameters of a generic class.
        /// </summary>
        public List<GenericParameter> GenericParameters { get; } = [];

        public List<Constructor> Constructors { get; } = [];

        public StaticConstructor? StaticConstructor { get; set; }

        public List<Field> Fields { get; } = [];

        public List<Property> Properties { get; } = [];

        public List<Method> Methods { get; } = [];

        public List<DotNetDefinition> NestedDefinitions { get; } = [];
    }
}
