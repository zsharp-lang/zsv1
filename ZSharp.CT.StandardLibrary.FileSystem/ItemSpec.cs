using ZSharp.Runtime.NET.IL2IR;

namespace Standard.FileSystem
{
    public abstract class ItemSpec(Path path)
    {
        public readonly Path path = path;

        public ItemSpec(string path)
            : this(new Path(path)) { }

        [Alias(Name = "toString")]
        public override string ToString()
            => $"Path<{path}>";
    }
}
