using ZSharp.Compiler.Features.Callable;

namespace ZSharp.SourceCompiler.Objects
{
    partial class Constructor
    {
        internal readonly Signature signature = new();

        public ISignature Signature => signature;
    }
}
