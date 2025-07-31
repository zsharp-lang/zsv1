using Standard.FileSystem;

namespace ZSharp.SourceCompiler.Project
{
    public class SubModule(Directory directory)
    {
        public Directory Directory { get; } = directory;

        public List<File> SourceFiles { get; } = [];

        public List<SubModule> SubModules { get; } = [];

        public required File SubModuleFile { get; init; }
    }
}
