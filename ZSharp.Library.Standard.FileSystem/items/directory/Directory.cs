namespace Standard.FileSystem
{
    public sealed partial class Directory
        : Item
    {
        private Directory(RawPath raw) : base(raw)
        {

        }

        [Alias("createDirectory")]
        public Directory CreateDirectory(string name, [NamedParameter] bool existsOk = false)
        {
            var path = (this / name).raw;

            if (!System.IO.Directory.Exists(path))
                System.IO.Directory.CreateDirectory(path);
            else if (!existsOk) throw new InvalidOperationException();

            return new(path);
        }

        [Alias("cwd")]
        public static Directory CurrentWorkingDirectory()
            => new(System.IO.Directory.GetCurrentDirectory());
    }
}
