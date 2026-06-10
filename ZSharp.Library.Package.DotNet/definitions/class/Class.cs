using ZSharp.HIR;

namespace Package.DotNet
{
    public sealed partial class Class
        : Definition
        , ZSharp.HIR.Type
        , ClassReference
        , DotNetDefinition
    {
        public List<Field> Fields { get; } = [];

        public List<Method> Methods { get; } = [];

        public List<Property> Properties { get; } = [];

        public bool IsAbstract { get; set; } = false;

        public bool IsSealed { get; set; } = false;

        public bool IsStatic => IsAbstract && IsSealed;

        public List<DotNetDefinition> NestedDefinitions { get; } = [];
    }
}
