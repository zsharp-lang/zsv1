namespace ZSharp.HIR.Code
{
    public sealed class Index
        : Expression
    {
        public required Expression Target { get; set; }

        public List<Expression> Arguments { get; set; } = [];
    }
}
