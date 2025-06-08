using ZSharp.Runtime.NET.IL2IR;

namespace Standard.FileSystem
{
    public class Path
    {
        internal string pathString;

        internal Path(string path)
        {
            this.pathString = path;
        }

        //[Alias(Name = "asDirectory")]
        //public Directory? AsDirectory()
        //{
        //    if (System.IO.Directory.Exists(path))
        //        return new(path);

        //    return null;
        //}

        //[Alias(Name = "asFile")]
        //public File? AsFile()
        //{
        //    if (System.IO.File.Exists(path))
        //        return new(path);

        //    return null;
        //}

        [Alias(Name = "sub")]
        public ItemSpec SubPath(string name)
            => Global_Operators.SubPath(this, name);

        [Alias(Name = "toString")]
        public override string ToString()
            => pathString;
    }
}
