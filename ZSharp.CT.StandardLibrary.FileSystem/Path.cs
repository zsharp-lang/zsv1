using ZSharp.Runtime.NET.IL2IR;

namespace Standard.FileSystem
{
    public class Path
    {
        internal string path;

        internal Path(string path)
        {
            this.path = path;
        }

        [Alias(Name = "asDirectory")]
        public Directory? AsDirectory()
        {
            if (System.IO.Directory.Exists(path))
                return new(path);

            return null;
        }

        [Alias(Name = "asFile")]
        public File? AsFile()
        {
            if (System.IO.File.Exists(path))
                return new(path);

            return null;
        }

        [Alias(Name = "sub")]
        public Path SubPath(string name)
            => Global_Operators.SubPath(this, name);

        [Alias(Name = "toString")]
        public override string ToString()
            => path;
    }
}
