using ZSharp.Text;

namespace ZSharp.AST
{
    public sealed class ExpressionTokens : TokenInfo
    {
        public required Token Semicolon { get; init; }
    }
}
