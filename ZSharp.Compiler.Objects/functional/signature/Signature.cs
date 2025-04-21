using ZSharp.Compiler;
using Parameters = CommonZ.Utils.Collection<ZSharp.Objects.Parameter>;

namespace ZSharp.Objects
{
    public sealed class Signature 
        : CompilerObject
        , IReferencable<Signature>
        , ISignature
    {
        public Parameters Args { get; init; } = [];

        public VarParameter? VarArgs { get; set; }

        public Parameters KwArgs { get; init; } = [];

        public KeywordVarParameter? VarKwArgs { get; set; }

        Signature IReferencable<Signature>.CreateReference(Referencing @ref, ReferenceContext context)
        {
            Signature result = new();

            result.Args.AddRange(Args.Select(arg => @ref.CreateReference<Parameter>(arg, context)));

            if (VarArgs is not null)
                result.VarArgs = @ref.CreateReference<VarParameter>(VarArgs, context);

            result.KwArgs.AddRange(KwArgs.Select(arg => @ref.CreateReference<Parameter>(arg, context)));

            if (VarKwArgs is not null)
                result.VarKwArgs = @ref.CreateReference<KeywordVarParameter>(VarKwArgs, context);

            return result;
        }

        IEnumerable<IParameter>? ISignature.GetArgs(Compiler.Compiler compiler)
            => Args;

        IEnumerable<IParameter>? ISignature.GetKwArgs(Compiler.Compiler compiler)
            => KwArgs;

        IVarParameter? ISignature.GetVarArgs(Compiler.Compiler compiler)
            => VarArgs;

        IKeywordVarParameter? ISignature.GetVarKwArgs(Compiler.Compiler compiler)
            => VarKwArgs;
    }
}
