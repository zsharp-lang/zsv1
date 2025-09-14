
namespace Standard.FileSystem
{
    public sealed class Location
    : Item
    {
        private Location(RawPath raw) : base(raw)
        {

        }

        public new static Location From(RawPath raw)
        {
            ArgumentNullException.ThrowIfNull(raw, nameof(raw));
            return new(raw);
        }
    }
}
