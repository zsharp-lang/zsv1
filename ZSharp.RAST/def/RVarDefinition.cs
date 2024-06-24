namespace ZSharp.RAST
{
    public sealed class RVarDefinition : RDefinition
    {
        public RExpression? Type { get; set; }

        public RExpression? Value { get; set; }

        public RVarDefinition(RId id, RExpression? type, RExpression? value)
            : base(id)
        {
            Type = type;
            Value = value;
        }

        public RVarDefinition(string name, RExpression? type, RExpression? value)
            :
             this(new RId(name), type, value)
        {

        }
    }
}
