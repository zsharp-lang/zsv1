using CommonZ.Utils;

namespace ZSharp.RAST
{
    public sealed class RObjectPattern : RPattern
    {
        public Mapping<string, RPattern> Patterns { get; }

        public RObjectPattern(Mapping<string, RPattern> patterns)
        {
            Patterns = patterns;
        }
    }
}
