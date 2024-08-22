using ZSharp.Text;

namespace ZSharp.AST
{
    public sealed class ImportTokens : TokenInfo
    {
        public required Token ImportKeyword { get; init; }

        public required Token Semicolon { get; init; }
    }
}
