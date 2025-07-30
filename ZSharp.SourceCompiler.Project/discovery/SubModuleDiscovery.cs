using Standard.FileSystem;

namespace ZSharp.SourceCompiler.Project
{
    public class SubModuleDiscovery(SubModule subModule)
    {
        public const string SubModuleFileName = ".zs";

        private readonly Dictionary<Directory, SubModule> _subModules = [];

        public SubModule SubModule { get; } = subModule;

        public void Discover()
        {
            Discover(SubModule.SourceDirectory, SubModule);
        }

        private void Discover(Directory directory, SubModule subModule)
        {
            _subModules[directory] = subModule;

            foreach (var item in directory)
                if (item is Directory subDirectory)
                    Discover(
                        subDirectory,
                        (subDirectory / SubModuleFileName) is File subModuleFule 
                        ? new(subDirectory.Name, subDirectory)
                            {
                                SubModuleFile = subModuleFule
                            } 
                        : subModule
                    );
                else if (item is not File file || file.Equals(subModule.SubModuleFile)) continue;
                else if (file.Extension != ".zs") continue;
                else subModule.SourceFiles.Add(file);
        }
    }
}
