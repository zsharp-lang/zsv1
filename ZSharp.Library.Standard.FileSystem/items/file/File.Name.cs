namespace Standard.FileSystem
{
    partial class File
    {
        public string Extension
        {
            get => System.IO.Path.GetExtension(raw);
            set => System.IO.File.Move(raw, (Parent / $"{Stem}{value}").raw);
        }

        public string Name
        {
            get => System.IO.Path.GetFileName(raw);
            set => System.IO.File.Move(raw, (Parent / value).raw);
        }

        public string Stem
        {
            get => System.IO.Path.GetFileNameWithoutExtension(raw);
            set => System.IO.File.Move(raw, (Parent / $"{value}{Extension}").raw);
        }
    }
}
