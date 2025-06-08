using ZSharp.Runtime.NET.IL2IR;

namespace Standard.FileSystem
{
    [ModuleGlobals]
    public static class Global_Operators
    {
        [Alias(Name = "_/_")]
        public static ItemSpec SubPath(Path path, string name)
        {
            var newPath = System.IO.Path.Combine(path.pathString, name);

            if (System.IO.Directory.Exists(newPath))
                return new Directory(newPath);
            if (System.IO.File.Exists(newPath))
                return new File(newPath);
            
            return new PathSpec(newPath);
        }

        [Alias(Name = "_/_")]
        public static ItemSpec SubPath(Directory directory, string name)
            => SubPath(directory.path, name);

        [Alias(Name = "_/_")]
        public static ItemSpec SubPath(PathSpec pathSpec, string name)
            => SubPath(pathSpec.path, name);
    }
}
