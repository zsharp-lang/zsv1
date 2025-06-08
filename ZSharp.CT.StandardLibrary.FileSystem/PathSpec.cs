namespace Standard.FileSystem
{
    public class PathSpec(Path path) : ItemSpec(path)
    {
        public PathSpec(string path) 
            : this(new Path(path))
        {
        }
    }
}
