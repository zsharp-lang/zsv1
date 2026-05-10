namespace ZSharp.HIR.Code
{
    public sealed class Call
        : Expression
    {
        public required Expression Callee { get; set; }

        public List<Argument> Arguments { get; set; } = [];
    }
}
