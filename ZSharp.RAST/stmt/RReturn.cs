namespace ZSharp.RAST
{
    public sealed class RReturn : RStatement
    {
        public RExpression? Value { get; set; }

        public RReturn(RExpression? value)
        {
            Value = value;
        }
    }
}
