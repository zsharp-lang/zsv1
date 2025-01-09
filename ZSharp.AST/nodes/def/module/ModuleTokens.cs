using ZSharp.Text;

namespace ZSharp.AST
{
    public sealed class ModuleTokens : TokenInfo
    {
        public Token ModuleKeyword { get; init; }

        public Token Name { get; init; }
    }
}
