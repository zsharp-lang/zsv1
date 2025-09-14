namespace Standard.FileSystem
{
    partial class Directory
    {
        public static Item operator /(Directory directory, RawPath path)
            => ModuleScope.Join(directory, path);

        public static Item operator/(Directory directory, RelativePath path)
            => ModuleScope.Join(directory, path.raw);
    }
}
