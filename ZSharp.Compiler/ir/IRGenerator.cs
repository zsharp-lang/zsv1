using CG = ZSharp.CGRuntime;

namespace ZSharp.Compiler
{
    internal class IRGenerator
    {
        private readonly CG.Runtime runtime;

        public IR.Module Compile(CG.Module module)
        {
            IR.Module ir = new(module.Name);



            return ir;
        }
    }
}
