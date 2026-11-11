namespace ZSharp.AST
{
    public sealed class CallExpression(CallTokens tokenInfo) : Expression(tokenInfo)
    {
        public new CallTokens TokenInfo => As<CallTokens>();

        public required Expression Callee { get; set; }

        public List<CallArgument> Arguments { get; set; } = [];
    }
}
