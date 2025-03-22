using ZSharp.Text;

namespace ZSharp.Parser
{
    public sealed class UnexpectedTokenError(
        string expectedToken, 
        Token gotToken
    ) : ParseError(FormatMessage(expectedToken, gotToken))
    {
        public Token GotToken { get; } = gotToken;

        private static string FormatMessage(
            string expectedToken,
            Token gotToken
        )
        {
            return $"Expected token '{expectedToken}' but got '{gotToken}'";
        }
    }
}
