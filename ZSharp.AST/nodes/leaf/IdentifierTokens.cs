using ZSharp.Text;

namespace ZSharp.AST
{
    public sealed class IdentifierTokens(Token identifier) : TokenInfo
    {
        public Token Identifier { get; } = identifier;
    }
}
