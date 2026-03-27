namespace ZSharp.AST
{
    public class OOPDefinition(OOPDefinitionTokens tokens) : Definition(tokens)
    {
        public new OOPDefinitionTokens TokenInfo
        {
            get => As<OOPDefinitionTokens>();
            init => base.TokenInfo = value;
        }

        public required string Type { get; set; }

        public string Name { get; set; } = string.Empty;

        public List<GenericParameter>? GenericParameters { get; set; }

        public Expression? Of { get; set; }

        public Signature? Signature { get; set; }

        public List<Expression>? Bases { get; set; }

        public BlockStatement? Content { get; set; }
    }
}
