using Standard.FileSystem;

namespace ZSharp.SourceCompiler.Project
{
    public sealed class ProjectDiscovery(Project project)
    {
        private readonly Dictionary<Directory, SubModule> _subModules = [];

        public Project Project { get; } = project;

        public void Discover()
        {
            
        }
    }
}
