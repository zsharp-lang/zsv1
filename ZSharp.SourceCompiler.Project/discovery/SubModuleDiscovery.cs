using Standard.FileSystem;

namespace ZSharp.SourceCompiler.Project
{
    public static class SubModuleDiscovery
    {
        public const string SubModuleFileName = ".zs";

        public static void Discover(SubModule subModule)
        {
            Discover(subModule.Directory, subModule);
        }

        private static void Discover(Directory directory, SubModule subModule)
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
