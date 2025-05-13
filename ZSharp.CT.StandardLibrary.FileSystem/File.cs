using ZSharp.Runtime.NET.IL2IR;

namespace Standard.FileSystem
{
    public sealed class File
    {
        internal Path path;

        internal File(Path path)
        {
            this.path = path;
        }

        internal File(string path)
            : this(new Path(path)) { }

        [Alias(Name = "toString")]
        public override string ToString()
            => $"File<{path}>";
    }
}
