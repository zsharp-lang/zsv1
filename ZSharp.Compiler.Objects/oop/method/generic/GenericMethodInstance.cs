using ZSharp.Compiler;
namespace ZSharp.Objects
{
    public sealed class GenericMethodInstance(GenericMethod origin)
        : CompilerObject
        , ICTCallable_Old
        , ICompileIRReference<IR.ConstructedMethod>
        , IRTBoundMember
    {
        public GenericMethod Origin { get; } = origin;

        public required ReferenceContext Context { get; init; }

        public required CompilerObject Owner { get; init; }

        public required Signature Signature { get; init; }

        CompilerObject IRTBoundMember.Bind(Compiler.Compiler compiler, CompilerObject value)
            => new BoundGenericMethodInstance(this, value);

        CompilerObject ICTCallable_Old.Call(Compiler.Compiler compiler, Argument[] arguments)
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
                new IR.VM.Call(compiler.CompileIRReference<IR.ConstructedMethod>(this))
            ]));

            code.Types.Clear();
            if (Origin.ReturnType != compiler.TypeSystem.Void)
                code.Types.Add(compiler.Feature<Referencing>().CreateReference<IType>(Origin.ReturnType, Context));

            return new RawCode(code);
        }

        IR.ConstructedMethod ICompileIRReference<IR.ConstructedMethod>.CompileIRReference(Compiler.Compiler compiler)
        {
            var result = new IR.ConstructedMethod(
                compiler.CompileIRObject<IR.Method, IR.Class>(Origin, null)
            )
            {
                OwningType = compiler.CompileIRReference<IR.OOPTypeReference>(Owner)
            };

            foreach (var genericParameter in Origin.GenericParameters)
                result.Arguments.Add(
                    compiler.CompileIRType(Context.CompileTimeValues.Cache(genericParameter) ?? throw new())
                );

            return result;
        }
    }
}
