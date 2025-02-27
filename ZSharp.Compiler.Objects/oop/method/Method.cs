using System.Diagnostics.CodeAnalysis;
using ZSharp.Compiler;

namespace ZSharp.Objects
{
    public sealed class Method(string? name)
        : CompilerObject
        , IRTBoundMember
        , ICTCallable
        , ICompileIRObject<IR.Method, IR.Class>
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

        public IR.Method? IR { get; set; }

        public string? Name { get; set; } = name;

        public Signature Signature { get; set; } = new();

        public CompilerObject? ReturnType { get; set; }

        public CompilerObject? Body { get; set; }

        public CompilerObject Bind(Compiler.Compiler compiler, CompilerObject value)
            => new BoundMethod(this, value);

        public CompilerObject Call(Compiler.Compiler compiler, Argument[] arguments)
        {
            var (args, kwargs) = Utils.SplitArguments(arguments);

            try
            {
                return Call(compiler, args, kwargs);
            }
            catch
            {
                throw new ArgumentMismatchException(this, arguments);
            }
        }

        public CompilerObject Call(Compiler.Compiler compiler, Args args, KwArgs kwArgs)
        {
            // TODO: type checking (when type system is implemented)

            IRCode
                argsCode = new(), varArgsCode = new(),
                kwArgsCode = new(), varKwArgsCode = new();

            if (args.Count > Signature.Args.Count)
                if (Signature.VarArgs is null)
                    throw new($"Function {Name} takes {Signature.Args.Count} arguments, but {args.Count} were given.");
                else
                    throw new NotImplementedException("var args");
            else if (args.Count < Signature.Args.Count)
                throw new($"Function {Name} takes {Signature.Args.Count} arguments, but {args.Count} were given.");

            for (int i = 0; i < Signature.Args.Count; i++)
                argsCode.Append(compiler.CompileIRCode(args[i]));

            if (kwArgs.Count > Signature.KwArgs.Count)
                if (Signature.VarKwArgs is null)
                    throw new($"Function {Name} takes {Signature.KwArgs.Count} keyword arguments, but {kwArgs.Count} were given.");
                else
                    throw new NotImplementedException("var kwargs");
            else if (kwArgs.Count < Signature.KwArgs.Count)
                throw new($"Function {Name} takes {Signature.KwArgs.Count} keyword arguments, but {kwArgs.Count} were given.");

            foreach (var kwArgParameter in Signature.KwArgs)
                kwArgsCode.Append(compiler.CompileIRCode(kwArgs[kwArgParameter.Name]));

            IRCode result = new();
            result.Append(argsCode);
            result.Append(varArgsCode); // should be empty
            result.Append(kwArgsCode);
            result.Append(varKwArgsCode); // should be empty

            result.Instructions.Add(new IR.VM.Call(IR!));

            result.Types.Clear();
            if (ReturnType != compiler.TypeSystem.Void)
                result.Types.Add(ReturnType!);

            result.MaxStackSize = Math.Max(result.MaxStackSize, result.Types.Count);

            return new RawCode(result);
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

            if (owner is not null && !state.Get(BuildState.Owner))
            {
                state.Set(BuildState.Owner);

                owner.Methods.Add(IR);
            }

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
    }
}
