namespace ZSharp.RAST
{
    public sealed class RPairPattern : RPattern
    {
        public RPattern Key { get; }

        public RPattern Value { get; }

        public RPairPattern(RPattern key, RPattern value)
        {
            Key = key;
            Value = value;
        }
    }
}
