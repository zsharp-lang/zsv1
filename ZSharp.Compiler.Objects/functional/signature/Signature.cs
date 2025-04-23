using ZSharp.Compiler;
using Parameters = CommonZ.Utils.Collection<ZSharp.Objects.Parameter>;

namespace ZSharp.Objects
{
    public sealed class Signature 
        : CompilerObject
        , ICompileIRObject<IR.Signature, IR.Signature>
        , IReferencable<Signature>
        , ISignature
    {
        public IR.Signature? IR { get; private set; }

        public Parameters Args { get; init; } = [];

        public VarParameter? VarArgs { get; set; }

        public Parameters KwArgs { get; init; } = [];

        public KeywordVarParameter? VarKwArgs { get; set; }

        IR.Signature ICompileIRObject<IR.Signature, IR.Signature>.CompileIRObject(Compiler.Compiler compiler, IR.Signature? owner)
        {
            if (IR is not null)
                return IR;

            owner ??= new(compiler.RuntimeModule.TypeSystem.Void);

            IR = owner;

            foreach (var arg in Args)
                IR.Args.Parameters.Add(compiler.CompileIRObject<IR.Parameter, IR.Signature>(arg, IR));

            if (VarArgs is not null)
                IR.Args.Var = compiler.CompileIRObject<IR.Parameter, IR.Signature>(VarArgs, IR);

            foreach (var kwArg in KwArgs)
                IR.KwArgs.Parameters.Add(compiler.CompileIRObject<IR.Parameter, IR.Signature>(kwArg, IR));

            if (VarKwArgs is not null)
                IR.KwArgs.Var = compiler.CompileIRObject<IR.Parameter, IR.Signature>(VarKwArgs, IR);

            return IR;
        }

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
