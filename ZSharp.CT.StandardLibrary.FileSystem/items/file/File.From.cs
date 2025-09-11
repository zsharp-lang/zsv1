namespace Standard.FileSystem
{
    partial class File
    {
        public new static File? From(RawPath raw)
        {
            ArgumentNullException.ThrowIfNull(raw, nameof(raw));
            if (System.IO.File.Exists(raw))
                return new(raw);
            return null;
        }
    }
}
