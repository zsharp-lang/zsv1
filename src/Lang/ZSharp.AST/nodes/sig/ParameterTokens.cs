using ZSharp.Text;

namespace ZSharp.AST
{
    public sealed class ParameterTokens : TokenInfo
    {
        public Token Name { get; init; }

        public Token? AsKeyword { get; init; }

        public Token? TypeSeparator { get; init; }

        public Token? ValueSeparator { get; init; }
    }
}
