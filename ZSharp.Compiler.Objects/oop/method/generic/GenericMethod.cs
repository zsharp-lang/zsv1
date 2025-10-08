using CommonZ.Utils;
using System.Diagnostics.CodeAnalysis;
using ZSharp.Compiler;

namespace ZSharp.Objects
{
    public sealed class GenericMethod(string? name)
        : CompilerObject
        , ICompileIRObject<IR.Method, IR.Class>
        , ICompileIRObject<IR.Method, IR.TypeDefinition>
        , ICTGetIndex
        , INamedObject
        , IReferencable<GenericMethodInstance>
        , IRTBoundMember
    {
        #region Build State

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

        #endregion

        #region Definition

        public string Name { get; set; } = name ?? string.Empty;

        public Collection<GenericParameter> GenericParameters { get; set; } = [];

        public Signature Signature { get; set; } = new();

        public CompilerObject? Owner { get; set; }

        public IType? ReturnType
        {
            get => Signature.ReturnType;
            set => Signature.ReturnType = value;
        }

        public CompilerObject? Body { get; set; }

        #endregion

        #region IR

        public IR.Method? IR { get; set; }

        IR.IRDefinition ICompileIRObject.CompileIRObject(Compiler.Compiler compiler)
            => CompileIR(compiler);

        IR.Method ICompileIRObject<IR.Method, IR.Class>.CompileIRObject(Compiler.Compiler compiler, IR.Class? owner)
        {
            CompileIR(compiler);

            if (owner is not null && !state[BuildState.Owner])
            {
                state[BuildState.Owner] = true;

                owner.Methods.Add(IR);
            }

            CompileDefinition(compiler);

            return IR;
        }

        IR.Method ICompileIRObject<IR.Method, IR.TypeDefinition>.CompileIRObject(Compiler.Compiler compiler, IR.TypeDefinition? owner)
        {
            CompileIR(compiler);

            CompileDefinition(compiler);

            return IR;
        }

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
                    Name = Name
                };
        }

        private void CompileDefinition(Compiler.Compiler compiler)
        {
            if (IR is null)
                throw new InvalidOperationException();

            if (!state[BuildState.Generic])
            {
                state[BuildState.Generic] = true;

                foreach (var genericParameter in GenericParameters)
                    IR.GenericParameters.Add(
                        compiler.CompileIRType<IR.GenericParameter>(genericParameter)
                    );
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
        }

        #endregion

        #region Index

        CompilerObject ICTGetIndex.Index(Compiler.Compiler compiler, Argument[] index)
        {
            var context = new ReferenceContext();

            foreach (var (genericParameter, genericArgument) in GenericParameters.Zip(index))
                context[genericParameter] = genericArgument.Object;

            return compiler.Feature<Referencing>().CreateReference(this, context);
        }

        #endregion

        #region Reference

        GenericMethodInstance IReferencable<GenericMethodInstance>.CreateReference(Referencing @ref, ReferenceContext context)
        {
            int currentErrors = @ref.Compiler.Log.Logs.Count(l => l.Level == LogLevel.Error);

            foreach (var genericParameter in GenericParameters)
                if (!context.CompileTimeValues.Contains(genericParameter))
                    @ref.Compiler.Log.Error(
                        $"Missing generic argument for parameter {genericParameter.Name} in type {Name}",
                        this
                    );

            if (@ref.Compiler.Log.Logs.Count(l => l.Level == LogLevel.Error) > currentErrors)
                throw new(); // TODO: Huh???

            return new GenericMethodInstance(this)
            {
                Context = new(context)
                {
                    Scope = this
                },
                Signature = @ref.CreateReference<Signature>(Signature, context),
                Owner = Owner ?? Signature.Args.First()?.Type ?? throw new()
            };
        }

        CompilerObject IRTBoundMember.Bind(Compiler.Compiler compiler, CompilerObject value)
            => new BoundGenericMethod(this, value);

        #endregion
    }
}
