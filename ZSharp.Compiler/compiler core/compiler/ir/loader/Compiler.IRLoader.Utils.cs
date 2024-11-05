using CommonZ.Utils;
using ZSharp.CGObjects;

namespace ZSharp.Compiler
{
    public sealed partial class Compiler
    {
        public bool IsImported(IR.IRObject ir)
            => ObjectCache.CG(ir) is not null;
    }
}
