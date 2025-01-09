using ZSharp.Text;

namespace ZSharp.AST
{
    public sealed class OOPDefinitionTokens : TokenInfo
    {
        public Token Type { get; init; }

        public Token? Name { get; init; }

        public Token? OfKeyword { get; init; }

        public Token? BasesSeparator { get; init; }
    }
}
