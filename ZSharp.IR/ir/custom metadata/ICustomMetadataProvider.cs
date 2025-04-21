using CommonZ.Utils;

namespace ZSharp.IR
{
    public interface ICustomMetadataProvider
    {
        public Collection<CustomMetadata> CustomMetadata { get; }

        public bool HasCustomMetadata => CustomMetadata.Count > 0;
    }
}
