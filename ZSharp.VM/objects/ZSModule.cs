using ZSharp.IR;

namespace ZSharp.VM
{
    public sealed class ZSModule(Module module, int globalCount, ZSObject type) 
        : ZSIRObject<Module>(module, globalCount, type)
    {
        public ZSObject GetGlobal(Global global)
            => GetField(global.Index);

        public void SetGlobal(Global global, ZSObject value)
            => SetField(global.Index, value);

        public static ZSModule CreateFrom(Module module, ZSObject type)
            => new(module, module.HasGlobals ? module.Globals.Count : 0, type);
    }
}
