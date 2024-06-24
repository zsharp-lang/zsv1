using ZSharp.RAST;

namespace ZSharp.CTRuntime
{
    internal sealed class ModuleCompiler(ZSCompiler compiler) : CompilerBase(compiler)
    {
        private readonly ModuleBodyCompiler moduleBodyCompiler = new(compiler);

        public IR.Module? Module => moduleBodyCompiler.IR;

        public RModule? ModuleNode => moduleBodyCompiler.Node;

        public IR.Module Compile(RModule module)
        {
            var moduleBinding = Context.GetCT<Module>(module);
            if (moduleBinding is null)
                moduleBinding = Context.Set<Module>(module, new(new(module.Name)));

            var moduleIR = moduleBinding.IR;

            moduleBodyCompiler.Compile(module, moduleIR);

            return moduleIR;
        }
    }
}
