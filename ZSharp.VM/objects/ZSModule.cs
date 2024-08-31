using ZSharp.IR;

namespace ZSharp.VM
{
    public sealed class ZSModule(Module module) 
        : ZSStruct(module.Globals.Count, TypeSystem.Module)
        //, IIRObject
    {
        public Module Module { get; } = module;

        public IRObject IR => Module;

        public ZSObject GetGlobal(Global global)
            => GetField(global.Index);

        public void SetGlobal(Global global, ZSObject value)
            => SetField(global.Index, value);
    }
}
