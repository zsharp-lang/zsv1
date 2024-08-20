using ZSharp.CGRuntime;
using ZSharp.RAST;

namespace ZSharp.CGCompiler
{
    public sealed class Compiler
    {
        private readonly Context context = new();

        private readonly ModuleCompiler moduleCompiler;

        public Compiler()
        {
            moduleCompiler = new(context);
        }

        public CGObjects.Module Compile(RModule module)
        {
            return moduleCompiler.Compile(module);
        }
    }
}
