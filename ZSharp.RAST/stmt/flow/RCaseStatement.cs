namespace ZSharp.RAST
{
    public sealed class RCaseWhen(RExpression value, RStatement body) : RNode
    {
        public string? Name { get; set; }

        public RExpression Value { get; set; } = value;

        public RStatement Body { get; set; } = body;
    }

    public sealed class RCaseStatement : RStatement
    {
        public RExpression? Value { get; set; }

        public RExpression? Of { get; set; }

        public List<RCaseWhen> Cases { get; set; } = [];

        public RStatement? Else { get; set; }
    }
}
