namespace Standard.FileSystem
{
    public sealed partial class File
        : Item
    {
        private File(RawPath raw) : base(raw)
        {

        }

        [Alias("getContent")]
        public string GetContent()
            => System.IO.File.ReadAllText(raw);
    }
}
