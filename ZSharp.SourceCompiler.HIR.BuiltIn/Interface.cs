namespace ZSharp.SourceCompiler.HIR.BuiltIn
{
    public class Interface
        : Node
    {
        public string Name { get; set; } = string.Empty;

        public List<Method> Methods { get; init; } = [];
    }
}
