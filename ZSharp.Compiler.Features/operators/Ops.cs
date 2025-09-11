using CommonZ.Utils;
using ZSharp.Objects;

namespace ZSharp.Compiler
{
    public sealed class Ops(Compiler compiler) : Feature(compiler)
    {
        public Cache<string, CompilerObject> Binary = [];
    }
}
