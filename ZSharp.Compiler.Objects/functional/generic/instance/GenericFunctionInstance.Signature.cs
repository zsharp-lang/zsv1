using ZSharp.Compiler;

namespace ZSharp.Objects
{
    partial class GenericFunctionInstance
    {
        public required ISignature Signature { get; init; }

        public IType? ReturnType => Signature.ReturnType;
    }
}
