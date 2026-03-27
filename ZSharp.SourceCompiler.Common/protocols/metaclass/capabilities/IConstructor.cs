using ZSharp.Compiler.Features.Callable;

namespace ZSharp.SourceCompiler
{
    public interface IConstructor
    {
        public ISignature Signature { get; }
    }
}
