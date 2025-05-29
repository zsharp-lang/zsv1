using CommonZ.Utils;
using ZSharp.Compiler;

namespace ZSharp.Objects
{
    public sealed class GenericFunction(string? name = null)
        : CompilerObject
        , ICompileIRObject<IR.Function, IR.Module>
        , ICTCallable
        , ICTGetIndex
        , IReferencable<GenericFunctionInstance>
    {
        [Flags]
        enum BuildState
        {
            None = 0,
            Signature = 0b1,
            Body = 0b10,
            Owner = 0b100,
            Generic = 0b1000,
        }

        private readonly ObjectBuildState<BuildState> state = new();

        public bool Defined
        {
            init
            {
                if (value)
                    foreach (var item in Enum.GetValues<BuildState>())
                        state[item] = true;
            }
        }

        public IR.Function? IR { get; set; }

        public string Name { get; set; } = name ?? string.Empty;

        public Collection<GenericParameter> GenericParameters { get; set; } = [];

        public IType? ReturnType
        {
            get => Signature.ReturnType;
            set => Signature.ReturnType = value;
        }

        public Signature Signature { get; set; } = new();

        public CompilerObject? Body { get; set; }

        IR.Function ICompileIRObject<IR.Function, IR.Module>.CompileIRObject(Compiler.Compiler compiler, IR.Module? owner)
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
                Name = Name
            };

            if (owner is not null && !state[BuildState.Owner])
            {
                state[BuildState.Owner] = true;

                owner.Functions.Add(IR);
            }

            if (!state[BuildState.Generic])
            {
                state[BuildState.Generic] = true;

                foreach (var genericParameter in GenericParameters)
                    IR.GenericParameters.Add(compiler.CompileIRType<IR.GenericParameter>(genericParameter));
            }

            if (!state[BuildState.Signature])
            {
                state[BuildState.Signature] = true;

                foreach (var arg in Signature.Args)
                    IR.Signature.Args.Parameters.Add(compiler.CompileIRObject<IR.Parameter, IR.Signature>(arg, IR.Signature));

                if (Signature.VarArgs is not null)
                    IR.Signature.Args.Var = compiler.CompileIRObject<IR.Parameter, IR.Signature>(Signature.VarArgs, IR.Signature);

                foreach (var kwArg in Signature.KwArgs)
                    IR.Signature.KwArgs.Parameters.Add(compiler.CompileIRObject<IR.Parameter, IR.Signature>(kwArg, IR.Signature));

                if (Signature.VarKwArgs is not null)
                    IR.Signature.KwArgs.Var = compiler.CompileIRObject<IR.Parameter, IR.Signature>(Signature.VarKwArgs, IR.Signature);
            }

            if (Body is not null && !state[BuildState.Body])
            {
                state[BuildState.Body] = true;

                IR.Body.Instructions.AddRange(compiler.CompileIRCode(Body).Instructions);
            }

            return IR;
        }

        CompilerObject ICTGetIndex.Index(Compiler.Compiler compiler, Argument[] index)
        {
            if (index.Length != GenericParameters.Count)
                throw new();

            ReferenceContext context = new();

            foreach (var (arg, param) in index.Zip(GenericParameters))
                context[param] = compiler.Evaluate(arg.Object);

            return compiler.Feature<Referencing>().CreateReference(this, context);
        }

        GenericFunctionInstance IReferencable<GenericFunctionInstance>.CreateReference(Referencing @ref, ReferenceContext context)
        {
            foreach (var genericParameter in GenericParameters)
                if (!context.CompileTimeValues.Contains(genericParameter))
                    throw new();

            return new(this)
            {
                Context = context,
                Signature = @ref.CreateReference<Signature>(Signature, context)
            };
        }

        CompilerObject ICTCallable.Call(Compiler.Compiler compiler, Argument[] arguments)
        {
            ITypeInferenceContext infer;

            using var _ = compiler.ContextScope(infer = new SimpleInferenceContext(compiler));

            List<InferredType> inferredTypes = [];
            ReferenceContext context = new();

            foreach (var genericParameter in GenericParameters)
            {
                var inferredType = infer.CreateInferredType();
                inferredTypes.Add(inferredType);
                context.CompileTimeValues.Cache(genericParameter, inferredType);
            }

            var reference = compiler.Feature<Referencing>().CreateReference<GenericFunctionInstance>(this, context);

            var args = (reference.Signature as ISignature)!.MatchArguments(compiler, arguments);

            foreach (var genericParameter in GenericParameters)
                context.CompileTimeValues.Cache(
                    genericParameter,
                    context.CompileTimeValues.Cache<InferredType>(genericParameter)!.Resolve(),
                    set: true
                );

            return compiler.Call(
                compiler.Feature<Referencing>().CreateReference(this, context), 
                arguments
            );
        }
    }
}
