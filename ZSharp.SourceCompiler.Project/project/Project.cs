using Standard.FileSystem;

namespace ZSharp.SourceCompiler.Project
{
    public sealed class Project(
        string name, 
        Directory rootDirectory, 
        Directory? sourceDirectory = null,
        File? subModuleFile = null
    )
    {
        public string Name { get; } = name;

        public Directory RootDirectory { get; } = rootDirectory;

        public Directory SourceDirectory => RootModule.Directory;

        public SubModule RootModule { get; } = new(sourceDirectory ?? rootDirectory)
        {
            SubModuleFile = 
                subModuleFile 
                ?? (sourceDirectory ?? rootDirectory) / SubModuleDiscovery.SubModuleFileName as File 
                ?? throw new System.ArgumentException(
                    $"Could not find file at {(sourceDirectory ?? rootDirectory) / SubModuleDiscovery.SubModuleFileName}", nameof(subModuleFile)
                )
        };

        public IEnumerable<SubModule> GetAllSubModules()
        {
            static IEnumerable<SubModule> GetSubModules(SubModule module)
            {
                yield return module;
                foreach (var subModule in module.SubModules)
                    foreach (var innerSubModule in GetSubModules(subModule))
                        yield return innerSubModule;
            }

            return GetSubModules(RootModule);
        }
    }
}
