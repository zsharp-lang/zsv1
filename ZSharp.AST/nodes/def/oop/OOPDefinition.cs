namespace ZSharp.AST
{
    public class OOPDefinition : Expression
    {
        public required string Type { get; set; }

        public string Name { get; set; } = string.Empty;

        public Expression? Of { get; set; }

        public Signature? Signature { get; set; }

        public List<Expression>? Bases { get; set; }

        public BlockStatement? Content { get; set; }
    }
}
