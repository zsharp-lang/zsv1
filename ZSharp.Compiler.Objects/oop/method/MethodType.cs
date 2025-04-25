using CommonZ.Utils;
using ZSharp.Compiler;

namespace ZSharp.Objects
{
    public sealed class MethodType(Signature signature, IType returnType)
        : CompilerObject
        , ICallableType
    {
        public Signature Signature { get; } = signature;

        Collection<IType> ICallableType.Args => [.. Signature.Args.Select(p => p.Type ?? throw new InvalidOperationException())];

        IType? ICallableType.VarArgs => Signature.VarArgs?.Type;

        Mapping<string, IType> ICallableType.KwArgs => new(Signature.KwArgs
            .ToDictionary(
                p => p.Name,
                p => p.Type ?? throw new InvalidOperationException()
            ));

        IType? ICallableType.VarKwArgs => Signature.VarKwArgs?.Type;

        IType ICallableType.ReturnType { get; } = returnType;
    }
}
