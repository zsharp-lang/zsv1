using ZSharp.CGObjects;

namespace ZSharp.Compiler
{
    internal sealed class ModuleOOPObject(ClassSpec spec)
        : CGObject
    {
        public CGObject? Object { get; set; }

        public ClassSpec Spec { get; } = spec;
    }
}
