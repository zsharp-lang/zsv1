namespace Standard.FileSystem
{
    partial class Directory
    {
        public string Name
        {
            get => System.IO.Path.GetFileName(raw);
            set => System.IO.Directory.Move(raw, (Parent / value).raw);
        }
    }
}
