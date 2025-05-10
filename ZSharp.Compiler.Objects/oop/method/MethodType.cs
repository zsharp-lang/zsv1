using CommonZ.Utils;
using ZSharp.Compiler;

namespace ZSharp.Objects
{
    public sealed class MethodType(Signature signature, IType returnType)
        : CompilerObject
        , ISignature
    {
        public Signature Signature { get; } = signature;

        IEnumerable<IParameter> ISignature.Args => Signature.Args;

        IVarParameter? ISignature.VarArgs => Signature.VarArgs;

        IEnumerable<IParameter> ISignature.KwArgs => Signature.KwArgs;

        IVarParameter? ISignature.VarKwArgs => Signature.VarKwArgs;

        IType ISignature.ReturnType => Signature.ReturnType;
    }
}
