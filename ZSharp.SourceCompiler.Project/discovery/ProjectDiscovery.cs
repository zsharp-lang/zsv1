namespace ZSharp.SourceCompiler.Project
{
    public static class ProjectDiscovery
    {
        public static void Discover(Project project)
        {
            SubModuleDiscovery.Discover(project.RootModule);
        }
    }
}
