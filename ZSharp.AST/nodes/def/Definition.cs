namespace ZSharp.AST
{
    public abstract class Definition(TokenInfo? tokens = null) : Expression(tokens)
    {
        public Expression? MetaType { get; set; }
    }
}
