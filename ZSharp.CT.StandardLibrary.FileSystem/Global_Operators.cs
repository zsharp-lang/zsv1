using ZSharp.Runtime.NET.IL2IR;

namespace Standard.FileSystem
{
    [ModuleGlobals]
    public static class Global_Operators
    {
        [Alias(Name = "_/_")]
        public static Path SubPath(Path path, string name)
            => new(System.IO.Path.Combine(path.path, name));

        [Alias(Name = "_/_")]
        public static Path SubPath(Directory directory, string name)
            => SubPath(directory.path, name);
    }
}
