using CommonZ.Utils;
using ZSharp.VM;

namespace ZSharp.IR.Standard
{
    public sealed class ModuleImporter : IStringImporter
    {
        private readonly Mapping<string, ZSModule> modules = new();

        public ZSModule AddModule(Module module, string? alias = null)
        {
            var zsModule = new ZSModule(module);
            modules.Add(alias ?? module.Name ?? throw new Exception("Unnamed module must be added with alias"), zsModule);
            return zsModule;
        }

        public Module GetModuleIR(string name)
        {
            return GetModule(name).Module;
        }

        public ZSModule GetModule(string name)
        {
            return modules[name];
        }

        public ZSObject Import(string source)
        {
            return GetModule(source);
        }
    }
}
