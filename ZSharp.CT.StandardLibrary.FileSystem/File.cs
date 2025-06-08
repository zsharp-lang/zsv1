using ZSharp.Runtime.NET.IL2IR;

namespace Standard.FileSystem
{
    public sealed class File(Path path) : ItemSpec(path)
    {
        internal File(string path)
            : this(new Path(path)) { }

        [Alias(Name = "toString")]
        public override string ToString()
            => $"File<{path}>";

        [Alias(Name = "getContent")]
        public string GetContent()
            => System.IO.File.ReadAllText(path.pathString);
    }
}
