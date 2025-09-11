namespace ZSharp.ZSSourceCompiler
{
    public partial class ZSSourceCompiler
    {
        public ImportSystem ImportSystem { get; } = new();

        public StringImporter StringImporter { get; }

        public StandardLibraryImporter StandardLibraryImporter { get; }

        public ZSImporter ZSImporter { get; }
    }
}
