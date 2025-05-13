using ZSharp.Runtime.NET.IL2IR;

namespace Standard.FileSystem
{
    public sealed class Directory
    {
        public Path path;

        internal Directory(Path path)
        {
            this.path = path;
        }

        internal Directory(string path)
            : this(new Path(path)) { }

        [Alias(Name = "sub")]
        public Path SubPath(string name)
            => Global_Operators.SubPath(this, name);

        [Alias(Name = "cwd")]
        public static Directory CurrentWorkingDirectory()
            => new(System.IO.Directory.GetCurrentDirectory());

        [Alias(Name = "toString")]
        public override string ToString()
            => $"Directory<{path}>";
    }
}
