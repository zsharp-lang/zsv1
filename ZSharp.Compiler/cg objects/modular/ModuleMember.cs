using ZSharp.Compiler;

namespace ZSharp.Objects
{
    public abstract class ModuleMember 
        : CompilerObject
        //, IImportable<Module>
    {
        public required Module Module { get; init; }
    }
}
