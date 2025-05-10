using ZSharp.Compiler;

namespace ZSharp.Objects
{
    public sealed class GenericMethodReference(
        GenericMethod origin,
        ReferenceContext context
    )
        : CompilerObject
        , ICTCallable
        , ICompileIRReference<IR.MethodReference>
        , IReferencable<GenericMethodInstance>
        , IRTBoundMember
    {
        public GenericMethod Origin { get; } = origin;

        public ReferenceContext Context { get; } = context;

        public required CompilerObject Owner { get; init; }

        public required Signature Signature { get; init; }

        public IR.Signature? SignatureIR { get; private set; }

        CompilerObject ICTCallable.Call(Compiler.Compiler compiler, Argument[] arguments)
        {
            if (Origin.ReturnType is null)
                throw new NotImplementedException();

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
                new IR.VM.Call(compiler.CompileIRReference<IR.MethodReference>(this))
            ]));

            code.Types.Clear();
            if (Origin.ReturnType != compiler.TypeSystem.Void)
                code.Types.Add(compiler.Feature<Referencing>().CreateReference<IType>(Origin.ReturnType, Context));

            return new RawCode(code);
        }

        IR.MethodReference ICompileIRReference<IR.MethodReference>.CompileIRReference(Compiler.Compiler compiler)
        {
            var type = compiler.Feature<Referencing>().CreateReference(Owner, Context);

            return new IR.MethodReference(compiler.CompileIRObject<IR.Method, IR.OOPType>(Origin, null!))
            {
                OwningType = compiler.CompileIRReference<IR.OOPTypeReference>(type),
                Signature = SignatureIR ?? CompileSignature(compiler)
            };
        }

        CompilerObject IRTBoundMember.Bind(Compiler.Compiler compiler, CompilerObject value)
            => new BoundGenericMethodReference(this, value);

        private IR.Signature CompileSignature(Compiler.Compiler compiler)
        {
            if (SignatureIR is not null)
                return SignatureIR;

            if (Origin.ReturnType is null)
                throw new NotImplementedException();

            SignatureIR = new(compiler.CompileIRType(compiler.Feature<Referencing>().CreateReference(Origin.ReturnType, Context)));

            foreach (var arg in Signature.Args)
                SignatureIR.Args.Parameters.Add(compiler.CompileIRObject<IR.Parameter, IR.Signature>(arg, SignatureIR));

            if (Signature.VarArgs is not null)
                SignatureIR.Args.Var = compiler.CompileIRObject<IR.Parameter, IR.Signature>(Signature.VarArgs, SignatureIR);

            foreach (var kwArg in Signature.KwArgs)
                SignatureIR.KwArgs.Parameters.Add(compiler.CompileIRObject<IR.Parameter, IR.Signature>(kwArg, SignatureIR));

            if (Signature.VarKwArgs is not null)
                SignatureIR.KwArgs.Var = compiler.CompileIRObject<IR.Parameter, IR.Signature>(Signature.VarKwArgs, SignatureIR);

            return SignatureIR;
        }

        #region Reference

        GenericMethodInstance IReferencable<GenericMethodInstance>.CreateReference(Referencing @ref, ReferenceContext context)
        {
            int currentErrors = @ref.Compiler.Log.Logs.Count(l => l.Level == LogLevel.Error);

            foreach (var genericParameter in Origin.GenericParameters)
                if (!context.CompileTimeValues.Contains(genericParameter))
                    @ref.Compiler.Log.Error(
                        $"Missing generic argument for parameter {genericParameter.Name} in type {Origin.Name}",
                        this
                    );

            if (@ref.Compiler.Log.Logs.Count(l => l.Level == LogLevel.Error) > currentErrors)
                throw new(); // TODO: Huh???

            return new GenericMethodInstance(Origin)
            {
                Context = new(context)
                {
                    Scope = this
                },
                Signature = @ref.CreateReference<Signature>(Signature, context),
                Owner = Owner ?? Signature.Args.First()?.Type ?? throw new()
            };
        }

        #endregion
    }
}
