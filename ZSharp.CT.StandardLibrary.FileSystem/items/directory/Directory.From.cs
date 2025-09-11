namespace Standard.FileSystem
{
    partial class Directory
    {
        public new static Directory? From(RawPath raw)
        {
            ArgumentNullException.ThrowIfNull(raw, nameof(raw));
            if (System.IO.Directory.Exists(raw))
                return new(raw);
            return null;
        }
    }
}
