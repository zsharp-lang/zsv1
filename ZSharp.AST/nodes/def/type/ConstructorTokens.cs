using ZSharp.Text;

namespace ZSharp.AST
{
    public sealed class ConstructorTokens : TokenInfo
    {
        public Token NewKeyword { get; init; }

        public Token? Name { get; init; }
    }
}
