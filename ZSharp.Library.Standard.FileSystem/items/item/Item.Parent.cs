namespace Standard.FileSystem
{
    partial class Item
    {
        private Directory? parent; 

        public Directory Parent
        {
            get
            {
                if (parent is not null) return parent;

                return parent = Directory.From(System.IO.Directory.GetParent(raw)!.FullName) ?? throw new();
            }
        }
    }
}
