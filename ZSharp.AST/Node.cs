namespace ZSharp.AST
{
    public class Node<T> where T : TokenInfo
    {
        public T TokenInfo { get; }

        public Node(T tokenInfo)
        {
            TokenInfo = tokenInfo;
        }
    }
}
