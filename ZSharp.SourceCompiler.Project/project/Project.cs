using Standard.FileSystem;

namespace ZSharp.SourceCompiler.Project
{
    public sealed class Project(
        string name, 
        Directory rootDirectory, 
        Directory? sourceDirectory = null
    ) 
        : SubModule(name, rootDirectory, sourceDirectory)
    {
    }
}
