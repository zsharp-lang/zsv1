namespace ZSharp.RAST
{
    public class RParameter : RDefinition
    {
        public RExpression? Type { get; set; }

        public RExpression? Default { get; set; }

        public RParameter(string name, RExpression? type, RExpression? @default)
            : this(new RId(name), type, @default)
        {

        }

        public RParameter(RId id, RExpression? type, RExpression? @default)
            : base(id)
        {
            Type = type;
            Default = @default;
        }
    }
}
