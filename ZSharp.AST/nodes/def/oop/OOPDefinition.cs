namespace ZSharp.AST
{
    public class OOPDefinition : Expression
    {
        public required string Type { get; set; }

        public string? Name { get; set; }

        public List<Expression>? Bases { get; set; }

        public BlockStatement? Content { get; set; }
    }
}
