using ZSharp.RAST;

namespace ZSharp.Compiler
{
    public sealed class Compiler
    {
        private readonly CGGenerator cg = new();
        private readonly IRGenerator ir;

        public IR.Module Compile(RStatement[] statements, string? moduleName = null)
        {
            var cgModule = cg.Compile(statements, moduleName);
            var irModule = ir.Compile(cgModule);

            return irModule;
        }
    }
}
