namespace ZSharp.SourceCompiler.HIR.BuiltIn
{
    public class Function
        : Node
    {
        public string Name { get; set; } = string.Empty;

        public List<Parameter> PositionalParameters { get; init; } = [];

        public List<Parameter> KeywordParameters { get; init; } = [];

        public Parameter? VariadicPositionalParameter { get; set; }

        public Parameter? VariadicKeywordParameter { get; set; }

        public Node? ReturnType { get; set; }

        public Node? Body { get; set; }
    }
}
