using CommonZ.Utils;
using ZSharp.Compiler;

namespace ZSharp.Objects
{
    public sealed class StubSignature
        : CompilerObject
        , ISignature
        , IReferencable<ISignature>
    {
        public Collection<StubParameter> Args { get; } = [];

        public StubVarParameter? VarArgs { get; set; }

        public Collection<StubParameter> KwArgs { get; } = [];

        public StubVarParameter? VarKwArgs { get; set; }

        public IType? ReturnType { get; set; }

        #region Signature

        IEnumerable<IParameter> ISignature.Args => Args;

        IVarParameter? ISignature.VarArgs => VarArgs;

        IEnumerable<IParameter> ISignature.KwArgs => KwArgs;

        IVarParameter? ISignature.VarKwArgs => VarKwArgs;

        IType ISignature.ReturnType => ReturnType ?? throw new();

        #endregion

        #region Reference

        ISignature IReferencable<ISignature>.CreateReference(Referencing @ref, ReferenceContext context)
        {
            StubSignature result = new();

            result.Args.AddRange(Args.Select(arg => @ref.CreateReference<StubParameter>(arg, context)));

            if (VarArgs is not null)
                result.VarArgs = @ref.CreateReference<StubVarParameter>(VarArgs, context);

            result.KwArgs.AddRange(KwArgs.Select(arg => @ref.CreateReference<StubParameter>(arg, context)));

            if (VarKwArgs is not null)
                result.VarKwArgs = @ref.CreateReference<StubVarParameter>(VarKwArgs, context);

            return result;
        }

        #endregion
    }
}
