namespace ZSharp.RAST
{
    public sealed class RVarStatement : RStatement
    {
        public RPattern Pattern { get; set; }

        public RExpression? Type { get; set; }

        public RExpression? Value { get; set; }

        public RVarStatement(RPattern pattern, RExpression? type, RExpression value)
        {
            Pattern = pattern;
            Type = type;
            Value = value;
        }
    }
}
