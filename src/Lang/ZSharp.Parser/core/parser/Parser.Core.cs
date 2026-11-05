using ZSharp.Text;

namespace ZSharp.Parser
{
    public sealed partial class Parser(TokenStream tokens)
    {
        public TokenStream TokenStream { get; } = tokens;

        public Parser(IEnumerable<Token> tokens)
            : this(new TokenStream(tokens)) { }
    }
}
