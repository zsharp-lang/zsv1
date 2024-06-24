namespace ZSharp.RAST
{
    public sealed class RLetDefinition : RDefinition
    {
        public RExpression? Type { get; set; }

        public RExpression Value { get; set; }

        public RLetDefinition(RId id, RExpression? type, RExpression value)
            : base(id)
        {
            Type = type;
            Value = value;
        }

        public RLetDefinition(string name, RExpression? type, RExpression value)
            :
             this(new RId(name), type, value)
        {

        }
    }
}
