using Standard.FileSystem;

namespace ZSharp.SourceCompiler.Project
{
    public class SubModuleDiscovery(SubModule subModule)
    {
        public const string SubModuleFileName = ".zs";

        public SubModule SubModule { get; } = subModule;

        public void Discover()
        {
            Discover(SubModule.Directory, SubModule);
        }

        private void Discover(Directory directory, SubModule subModule)
        {
            foreach (var item in directory)
                if (item is Directory subDirectory)
                {
                    var innerModule = subModule;
                    if (subDirectory / SubModuleFileName is File subModuleFule)
                        subModule.SubModules.Add(innerModule = new SubModule(subDirectory)
                        {
                            SubModuleFile = subModuleFule
                        });
                    Discover(subDirectory, innerModule);
                }
                else if (item is not File file || file.Equals(subModule.SubModuleFile)) continue;
                else if (file.Extension != ".zs") continue;
                else subModule.SourceFiles.Add(file);
        }
    }
}
