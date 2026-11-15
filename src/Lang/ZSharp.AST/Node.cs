namespace ZSharp.AST
{
    public abstract class Node(TokenInfo? tokenInfo = null)
    {
        public TokenInfo? TokenInfo { get; init; } = tokenInfo;

        /// <summary>
        /// Where the node was written, if it was parsed.
        /// </summary>
        public Location? Location { get; set; }

        protected T As<T>()
            where T : TokenInfo
            => (TokenInfo as T)!;
    }
}
