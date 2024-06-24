namespace ZSharp.Text
{
    public readonly struct Token
    {
        public TokenType Type { get; }

        public string Value { get; }

        public Span Span { get; }

        public Token(TokenType type, string value, Span span)
        {
            Type = type;
            Value = value;
            Span = span;
        }
    }
}
