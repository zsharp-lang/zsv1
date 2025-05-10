using CommonZ.Utils;
using System.Diagnostics.CodeAnalysis;
using ZSharp.Compiler;

namespace ZSharp.Objects
{
    public class RTFunction(string? name)
        : CompilerObject
        , ICTCallable
        , ICTReadable
        , ICompileIRObject<IR.Function, IR.Module>
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

        public bool IsDefined
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

        public CompilerObject? Body { get; set; }

        IType ITyped.Type => throw new NotImplementedException();

        public Signature Signature { get; set; } = new();

        public IType? ReturnType
        {
            get => Signature.ReturnType;
            set => Signature.ReturnType = value;
        }

        CompilerObject ICTCallable.Call(Compiler.Compiler compiler, Argument[] arguments)
        {
            return Call(compiler, arguments);
        }

        private RawCode Call(Compiler.Compiler compiler, Argument[] arguments)
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
                new IR.VM.Call(compiler.CompileIRObject<IR.Function, IR.Module>(this, null))
            ]));

            code.Types.Clear();
            if (ReturnType != compiler.TypeSystem.Void)
                code.Types.Add(ReturnType);

            return new RawCode(code);
        }

        IRCode ICTReadable.Read(Compiler.Compiler compiler)
            => new([
                new IR.VM.GetObject(IR!)
            ])
            {
                Types = [null!], // TODO: fix type
            };

        [MemberNotNull(nameof(ReturnType))]
        public IR.Function CompileIRObject(Compiler.Compiler compiler, IR.Module? owner)
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

            if (ReturnType is null)
                throw new PartiallyCompiledObjectException(this, Errors.UndefinedReturnType(Name));

            if (owner is not null && !state[BuildState.Owner])
            {
                state[BuildState.Owner] = true;

                owner.Functions.Add(IR);
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
    }
}
