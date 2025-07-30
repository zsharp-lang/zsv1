using Standard.FileSystem;

namespace ZSharp.SourceCompiler.Project
{
    public class SubModule(
        string name,
        Directory rootDirectory,
        Directory? sourceDirectory = null
    )
    {
        public string Name { get; } = name;

        public Directory RootDirectory { get; } = rootDirectory;

        public Directory SourceDirectory { get; } = sourceDirectory ?? rootDirectory;

        public List<File> SourceFiles { get; } = [];

        public List<SubModule> SubModules { get; } = [];

        public required File SubModuleFile { get; init; }
    }
}
