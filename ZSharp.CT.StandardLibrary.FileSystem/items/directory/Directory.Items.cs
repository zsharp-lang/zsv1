using System.Collections;

namespace Standard.FileSystem
{
    partial class Directory
        : IEnumerable<Item>
    {
        IEnumerator<Item> IEnumerable<Item>.GetEnumerator()
            => System.IO.Directory.EnumerateFileSystemEntries(raw)
                .Select(Item.From)
                .GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => (this as IEnumerable<Item>).GetEnumerator();
    }
}
