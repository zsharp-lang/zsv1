using ZSharp.Compiler;

namespace ZSharp.CGObjects
{
    public abstract class ModuleMember 
        : CGObject
        //, IImportable<Module>
    {
        public required Module Module { get; init; }
    }
}
