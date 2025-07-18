using CommonZ.Utils;
using System.Diagnostics.CodeAnalysis;
using ZSharp.Compiler;

using Args = CommonZ.Utils.Collection<ZSharp.Objects.CompilerObject>;
using KwArgs = CommonZ.Utils.Mapping<string, ZSharp.Objects.CompilerObject>;


namespace ZSharp.Objects
{
    public sealed class Method(string? name)
        : CompilerObject
        , IRTBoundMember
        , IMethod
        , ICTCallable_Old
        , ICompileIRObject<IR.Method, IR.Class>
        , ICompileIRObject<IR.Method, IR.OOPType>
        , ICompileIRReference<IR.MethodReference>
        , IImplementation
        , IImplementsSpecification
        , IImplicitCastToType
        , IReferencable<MethodReference>
        , ITyped
    {
        [Flags]
        enum BuildState
        {
            None = 0,
            Signature = 0b1,
            Body = 0b10,
            Owner = 0b100,
        }

        private readonly ObjectBuildState<BuildState> state = new();

        public bool Defined { init
            {
                if (value)
                    foreach (var item in Enum.GetValues<BuildState>())
                        state[item] = true;
            } 
        }

        public IR.Method? IR { get; set; }

        public string Name { get; set; } = name ?? string.Empty;

        public CompilerObject Owner { get; set; }

        public Signature Signature { get; set; } = new();

        public IType? ReturnType
        {
            get => Signature.ReturnType;
            set => Signature.ReturnType = value;
        }

        public CompilerObject? Body { get; set; }

        public Collection<(IAbstraction, CompilerObject)> Specifications { get; } = [];

        IType ITyped.Type
        {
            get
            {
                if (ReturnType is null)
                    throw new InvalidOperationException();

                return new MethodType(Signature, ReturnType);
            }
        }

        public CompilerObject Bind(Compiler.Compiler compiler, CompilerObject value)
            => new BoundMethod(this, value);

        public CompilerObject Call(Compiler.Compiler compiler, Argument[] arguments)
        {
            if (ReturnType is null)
                throw new PartiallyCompiledObjectException(this, Errors.UndefinedReturnType(Name));

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

            var ir = compiler.CompileIRReference<IR.MethodReference>(this);

            code.Append(new([
                IR.IsVirtual
                ? new IR.VM.CallVirtual(ir)
                : new IR.VM.Call(ir)
            ]));

            code.Types.Clear();
            if (ReturnType != compiler.TypeSystem.Void)
                code.Types.Add(ReturnType);

            return new RawCode(code);
        }

        [MemberNotNull(nameof(ReturnType))]
        public IR.Method CompileIRObject(Compiler.Compiler compiler, IR.Class? owner)
        {
            CompileIR(compiler);

            if (owner is not null && !state[BuildState.Owner])
            {
                state[BuildState.Owner] = true;

                owner.Methods.Add(IR);
            }

            CompileDefinition(compiler);

            ReturnType ??= null!;

            return IR;
        }

        MethodReference IReferencable<MethodReference>.CreateReference(Referencing @ref, ReferenceContext context)
        {
            if (context.Scope is not GenericClassInstance genericClassInstance)
                genericClassInstance = (GenericClassInstance)@ref.CreateReference(context.Scope, context);

            return new(this, context)
            {
                Owner = genericClassInstance,
                //ReturnType = ReturnType is null ? null : @ref.CreateReference(ReturnType, context),
                Signature = @ref.CreateReference<Signature>(Signature, context),
            };
        }

        IR.Method ICompileIRObject<IR.Method, IR.OOPType>.CompileIRObject(Compiler.Compiler compiler, IR.OOPType? owner)
        {
            CompileIR(compiler);

            CompileDefinition(compiler);

            return IR;
        }

        IR.IRObject ICompileIRObject.CompileIRObject(Compiler.Compiler compiler)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
            => $"<Method {Name}({Signature}): {ReturnType?.ToString() ?? "<Unknown>"}>";

        CompilerObject IImplicitCastToType.ImplicitCastToType(Compiler.Compiler compiler, IType type)
        {
            if (type is not ISignature callableType)
                throw new Compiler.InvalidCastException(this, type);

            throw new Compiler.InvalidCastException(this, type);
        }

        IR.MethodReference ICompileIRReference<IR.MethodReference>.CompileIRReference(Compiler.Compiler compiler)
            => new(
                compiler.CompileIRObject<IR.Method, IR.OOPType>(this, null)
            )
            {
                OwningType = compiler.CompileIRType<IR.OOPTypeReference>(Owner)
            };

        void IImplementsSpecification.OnImplementSpecification(Compiler.Compiler compiler, IAbstraction abstraction, CompilerObject specification)
            => Specifications.Add((abstraction, specification));

        [MemberNotNull(nameof(IR))]
        private IR.Method CompileIR(Compiler.Compiler compiler)
        {
            return IR ??=
                new(
                    compiler.CompileIRType(
                        ReturnType ?? throw new PartiallyCompiledObjectException(
                            this,
                            Errors.UndefinedReturnType(Name)
                        )
                    )
                )
                {
                    Name = Name,
                    IsInstance = true,
                    IsVirtual = Specifications.Count > 0,
                };
        }

        private void CompileDefinition(Compiler.Compiler compiler)
        {
            if (IR is null)
                throw new InvalidOperationException();

            if (!state[BuildState.Signature])
            {
                state[BuildState.Signature] = true;

                compiler.CompileIRObject<IR.Signature, IR.Signature>(Signature, IR.Signature);
            }

            if (Body is not null && !state[BuildState.Body])
            {
                state[BuildState.Body] = true;

                IR.Body.Instructions.AddRange(compiler.CompileIRCode(Body).Instructions);
            }
        }

        bool IImplementation.Implements(Compiler.Compiler compiler, CompilerObject specification, [NotNullWhen(true)] out IImplementsSpecification? implementation)
        {
            implementation = null;

            if (!compiler.TypeSystem.IsTyped<ISignature>(specification, out var specificationType))
                return (implementation = null) is not null;

            var args = (Argument[])[new Argument(new TypedUndefined(Signature.Args[0].Type ?? throw new()))]; // TODO: owner
            var specificationPartial = PartialCall.CreateFrom(compiler, specification, specificationType, args);
            var implementationPartial = PartialCall.CreateFrom(compiler, this, Signature, args);

            if (!compiler.TypeSystem.AreEqual(specificationPartial.Signature, implementationPartial.Signature))
                return false;

            implementation = this;
            return true;
        }
    }
}
