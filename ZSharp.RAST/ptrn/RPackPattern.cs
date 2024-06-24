namespace ZSharp.RAST
{
    public sealed class RPackPattern : RPattern
    {
        public RPattern Value { get; }

        public RPackPattern(RPattern value)
        {
            Value = value;
        }
    }
}
