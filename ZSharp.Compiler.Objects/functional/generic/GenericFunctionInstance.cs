using System.Diagnostics.CodeAnalysis;
using ZSharp.Compiler;

namespace ZSharp.Objects
{
    public sealed class GenericFunctionInstance(GenericFunction origin)
        : CompilerObject
        , ICTCallable
        , ICompileIRReference<IR.GenericFunctionInstance>
    {
        public GenericFunction Origin { get; } = origin;

        public required ReferenceContext Context { get; init; }

        public Signature? Signature { get; set; }

        public IR.Signature? CompiledSignature { get; private set; }

        CompilerObject ICTCallable.Call(Compiler.Compiler compiler, Argument[] arguments)
        {
            if (Origin.ReturnType is null)
                throw new NotImplementedException();

            CompileSignature(compiler);

            var args = (Signature as ISignature).MatchArguments(compiler, arguments);

            IRCode code = new();

            List<CompilerObject> @params = [];

            @params.AddRange(Signature.Args);

            if (Signature.VarArgs is not null)
                @params.Add(Signature.VarArgs);

            @params.AddRange(Signature.KwArgs);

            if (Signature.VarKwArgs is not null)
                @params.Add(Signature.VarKwArgs);

            foreach (var param in @params)
                code.Append(compiler.CompileIRCode(args[param]));

            code.Append(new([
                new IR.VM.Call(compiler.CompileIRReference<IR.GenericFunctionInstance>(this))
            ]));

            code.Types.Clear();
            if (Origin.ReturnType != compiler.TypeSystem.Void)
                code.Types.Add(compiler.Feature<Referencing>().CreateReference<IType>(Origin.ReturnType, Context));

            return new RawCode(code);
        }

        IR.GenericFunctionInstance ICompileIRReference<IR.GenericFunctionInstance>.CompileIRReference(Compiler.Compiler compiler)
        {
            var arguments = Origin.GenericParameters
                .Select(genericParameter => Context[genericParameter])
                .Select(compiler.CompileIRType);

            CompileSignature(compiler);

            return new(compiler.CompileIRObject<IR.Function, IR.Module>(Origin, null))
            {
                Arguments = [.. arguments],
                Signature = CompiledSignature,
            };
        }

        [MemberNotNull(nameof(Signature), nameof(CompiledSignature))]
        private void CompileSignature(Compiler.Compiler compiler)
        {
            Signature ??= compiler.Feature<Referencing>().CreateReference<Signature>(Origin.Signature, Context);

            if (CompiledSignature is not null)
                return;

            if (Origin.ReturnType is null)
                throw new NotImplementedException();

            CompiledSignature = new(compiler.CompileIRType(compiler.Feature<Referencing>().CreateReference(Origin.ReturnType, Context)));

            foreach (var arg in Signature.Args)
                CompiledSignature.Args.Parameters.Add(compiler.CompileIRObject<IR.Parameter, IR.Signature>(arg, CompiledSignature));

            if (Signature.VarArgs is not null)
                CompiledSignature.Args.Var = compiler.CompileIRObject<IR.Parameter, IR.Signature>(Signature.VarArgs, CompiledSignature);

            foreach (var kwArg in Signature.KwArgs)
                CompiledSignature.KwArgs.Parameters.Add(compiler.CompileIRObject<IR.Parameter, IR.Signature>(kwArg, CompiledSignature));

            if (Signature.VarKwArgs is not null)
                CompiledSignature.KwArgs.Var = compiler.CompileIRObject<IR.Parameter, IR.Signature>(Signature.VarKwArgs, CompiledSignature);
        }
    }
}
