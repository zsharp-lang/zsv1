namespace ZSharp.SourceCompiler.HIR.BuiltIn
{
    public class ClassBase
        : Node
        , IDefinition
    {
        public string Name { get; set; } = string.Empty;

        public List<IType> Interfaces { get; init; } = [];

        public List<Field> Fields { get; init; } = [];

        public List<Method> Methods { get; init; } = [];
    }
}
