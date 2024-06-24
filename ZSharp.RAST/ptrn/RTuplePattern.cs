namespace ZSharp.RAST
{
    public sealed class RTuplePattern : RPattern
    {
        public List<RPattern> Patterns { get; }

        public RTuplePattern(List<RPattern> patterns)
        {
            Patterns = patterns;
        }
    }
}
