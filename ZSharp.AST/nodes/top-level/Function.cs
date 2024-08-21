namespace ZSharp.AST
{
    public class Function(FunctionTokens tokens) : Node(tokens)
    {
        public new FunctionTokens TokenInfo
        {
            get => As<FunctionTokens>();
            init => base.TokenInfo = value;
        }
    }
}
