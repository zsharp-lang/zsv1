using ZSharp.RAST;

namespace ZSharp.CGCompiler
{
    public sealed class Compiler
    {
        private readonly Context context = new();

        private readonly ModuleCompiler moduleCompiler;

        public Compiler()
        {
            moduleCompiler = 
                context.CurrentCompiler as ModuleCompiler 
                ?? throw new Exception();
        }

        public CGObjects.Module Compile(RModule module)
        {
            return moduleCompiler.Compile(module);
        }
    }
}
