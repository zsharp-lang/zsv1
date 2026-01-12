namespace ZSharp.AST
{
    public class IdentifierExpression(IdentifierTokens tokens) : Expression
    {
        public new IdentifierTokens TokenInfo
        {
            get => As<IdentifierTokens>();
            init => base.TokenInfo = value;
        }

        public string Name { get; set; } = tokens.Identifier.Value;
    }
}
