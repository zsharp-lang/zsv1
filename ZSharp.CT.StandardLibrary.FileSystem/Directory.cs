using ZSharp.Runtime.NET.IL2IR;

namespace Standard.FileSystem
{
    public sealed class Directory(Path path) : ItemSpec(path)
    {
        public Directory(string path)
            : this(new Path(path)) { }

        [Alias(Name = "sub")]
        public ItemSpec SubPath(string name)
            => Global_Operators.SubPath(this, name);

        [Alias(Name = "createDirectory")]
        public Directory CreateDirectory(string name, [KeywordParameter] bool existsOk = false)
        {
            var path = this.path.SubPath(name).path;

            if (!System.IO.Directory.Exists(path.pathString))
                System.IO.Directory.CreateDirectory(path.pathString);
            else if (!existsOk) throw new InvalidOperationException();

            return new(path);
        }

        [Alias(Name = "cwd")]
        public static Directory CurrentWorkingDirectory()
            => new(System.IO.Directory.GetCurrentDirectory());

        [Alias(Name = "toString")]
        public override string ToString()
            => $"Directory<{path}>";
    }
}
