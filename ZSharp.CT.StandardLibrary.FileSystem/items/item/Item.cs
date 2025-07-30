namespace Standard.FileSystem
{
    public abstract partial class Item(RawPath raw)
    {
        protected internal readonly RawPath raw = raw ?? throw new ArgumentNullException(nameof(raw));

        [Alias("toString")]
        public override string ToString()
            => $"{GetType().Name}<{raw}>";

        internal static Item From(RawPath raw)
            => Directory.From(raw) as Item
            ?? File.From(raw) as Item
            ?? Location.From(raw);

        public override int GetHashCode()
            => (GetType(), raw).GetHashCode();

        public override bool Equals(object? obj)
        {
            if (obj is Item other)
            {
                return raw == other.raw && GetType() == other.GetType();
            }

            return false;
        }
    }
}
