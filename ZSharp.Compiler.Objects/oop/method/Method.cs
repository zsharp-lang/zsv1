using System.Diagnostics.CodeAnalysis;
using ZSharp.Compiler;

using Args = CommonZ.Utils.Collection<ZSharp.Objects.CompilerObject>;
using KwArgs = CommonZ.Utils.Mapping<string, ZSharp.Objects.CompilerObject>;


namespace ZSharp.Objects
{
    public sealed class Method(string? name)
        : CompilerObject
        , IRTBoundMember
        , ICTCallable
        , ICompileIRObject<IR.Method, IR.Class>
        , ICompileIRObject<IR.Method, IR.OOPType>
        , IReferencable<MethodReference>
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

        public Signature Signature { get; set; } = new();

        public CompilerObject? ReturnType { get; set; }

        public CompilerObject? Body { get; set; }

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

            code.Append(new([
                new IR.VM.Call(compiler.CompileIRObject<IR.Method, IR.OOPType>(this, null))
            ]));

            code.Types.Clear();
            if (ReturnType != compiler.TypeSystem.Void)
                code.Types.Add(ReturnType);

            return new RawCode(code);
        }

        [MemberNotNull(nameof(ReturnType))]
        public IR.Method CompileIRObject(Compiler.Compiler compiler, IR.Class? owner)
        {
            IR ??=
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
                };

            if (ReturnType is null)
                throw new PartiallyCompiledObjectException(
                    this, Errors.UndefinedReturnType(Name)
                );

            if (owner is not null && !state[BuildState.Owner])
            {
                state[BuildState.Owner] = true;

                owner.Methods.Add(IR);
            }

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
            IR ??=
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
                };

            if (!state.Get(BuildState.Signature))
            {
                state.Set(BuildState.Signature);

                foreach (var arg in Signature.Args)
                    IR.Signature.Args.Parameters.Add(compiler.CompileIRObject<IR.Parameter, IR.Signature>(arg, IR.Signature));

                if (Signature.VarArgs is not null)
                    IR.Signature.Args.Var = compiler.CompileIRObject<IR.Parameter, IR.Signature>(Signature.VarArgs, IR.Signature);

                foreach (var kwArg in Signature.KwArgs)
                    IR.Signature.KwArgs.Parameters.Add(compiler.CompileIRObject<IR.Parameter, IR.Signature>(kwArg, IR.Signature));

                if (Signature.VarKwArgs is not null)
                    IR.Signature.KwArgs.Var = compiler.CompileIRObject<IR.Parameter, IR.Signature>(Signature.VarKwArgs, IR.Signature);
            }

            if (Body is not null && !state.Get(BuildState.Body))
            {
                state.Set(BuildState.Body);

                IR.Body.Instructions.AddRange(compiler.CompileIRCode(Body).Instructions);
            }

            return IR;
        }

        IR.IRObject ICompileIRObject.CompileIRObject(Compiler.Compiler compiler)
        {
            throw new NotImplementedException();
        }
    }
}
