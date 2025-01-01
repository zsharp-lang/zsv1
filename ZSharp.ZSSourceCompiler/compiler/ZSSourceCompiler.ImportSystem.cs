namespace ZSharp.ZSSourceCompiler
{
    public partial class ZSSourceCompiler
    {
        public ImportSystem ImportSystem { get; } = new();

        public StringImporter StringImporter { get; } = new();

        public StandardLibraryImporter StandardLibraryImporter { get; } = new();

        public ZSImporter ZSImporter { get; } = new();
    }
}
