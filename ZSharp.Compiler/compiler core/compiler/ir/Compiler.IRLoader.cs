using CommonZ.Utils;
using ZSharp.CGObjects;

namespace ZSharp.Compiler
{
    public sealed partial class Compiler
    {
        private readonly ObjectCache objectCache = new();

        public CGObject ImportIR(IR.IRObject ir)
            => objectCache.CG(ir, out var result)
            ? result
            : objectCache.CG(ir, ir switch
            {
                IR.Module module => ImportIR(module),
                _ => throw new NotImplementedException(),
            });

        private Module ImportIR(IR.Module module)
        {
            Module result = new(module.Name!)
            {
                IR = module,
            };

            // TODO: populate module members with CG representations.

            return result;
        }
    }
}
