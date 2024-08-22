namespace ZSharp.AST
{
    public sealed class ExpressionStatement(ExpressionTokens tokenInfo) : Statement(tokenInfo)
    {
        public new ExpressionTokens TokenInfo => As<ExpressionTokens>();

        public required Expression Expression { get; set; }
    }
}
