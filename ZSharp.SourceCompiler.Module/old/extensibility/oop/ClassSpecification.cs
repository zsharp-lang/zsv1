namespace ZSharp.ZSSourceCompiler
{
    public sealed class ClassSpecification
    {
        public string Name { get; set; } = string.Empty;

        public Node[]? GenericParameters { get; set; }

        public Node[]? Parameters { get; set; }

        public Expression[] Bases { get; set; } = [];

        public Expression[] Content { get; set; } = [];
    }
}
