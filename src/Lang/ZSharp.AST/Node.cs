namespace ZSharp.AST
{
    public abstract class Node(TokenInfo? tokenInfo = null)
    {
        public TokenInfo? TokenInfo { get; init; } = tokenInfo;

        protected T As<T>()
            where T : TokenInfo
            => (TokenInfo as T)!;
    }
}
