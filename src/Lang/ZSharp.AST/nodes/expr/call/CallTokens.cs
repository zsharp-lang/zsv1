using ZSharp.Text;

namespace ZSharp.AST
{
    public sealed class CallTokens : TokenInfo
    {
        public required Token LParen { get; init; }

        public required Token RParen { get; init; }
    }
}
