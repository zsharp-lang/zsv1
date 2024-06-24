namespace ZSharp.RAST
{
    public class RArgument : RNode
    {
        public string? Name { get; set; }

        public RExpression Value { get; set; }

        public RArgument(RExpression value)
        {
            Value = value;
        }

        public RArgument(string? name, RExpression value)
            : this(value)
        {
            Name = name;
        }
    }
}
