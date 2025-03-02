using ZSharp.Compiler;

namespace ZSharp.Objects
{
    public sealed class Constructor(string? name)
        : CompilerObject
        , ICTCallable
        , ICompileIRObject<IR.Constructor, IR.Class>
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

        public IR.Constructor? IR { get; set; }

        public string? Name { get; set; } = name;

        public Signature Signature { get; set; } = new();

        public GenericClass? Owner { get; set; }

        public CompilerObject? Body { get; set; }

        public CompilerObject Call(Compiler.Compiler compiler, Argument[] arguments)
        {
            var (args, kwArgs) = Utils.SplitArguments(arguments);

            return Call(compiler, args, kwArgs);
        }

        public CompilerObject Call(Compiler.Compiler compiler, Args args, KwArgs kwArgs)
        {
            // TODO: type checking (when type system is implemented)

            IR.VM.Instruction invocationInstruction;

            bool hasReturn = false;
            if (kwArgs.TryGetValue(Signature.Args[0].Name, out var thisArgument))
            {
                invocationInstruction = new IR.VM.Call(IR!.Method);
                kwArgs.Remove(Signature.Args[0].Name);
                args.Insert(0, thisArgument);
            }
            else
            {
                if (Owner is null)
                    throw new();

                hasReturn = true;

                invocationInstruction = new IR.VM.CreateInstance(IR!);
                args.Insert(0, new RawCode(new()
                {
                    Types = [Owner]
                }));
            }

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

            result.Instructions.Add(invocationInstruction);

            result.Types.Clear();
            if (hasReturn)
                result.Types.Add(Owner ?? throw new());

            result.MaxStackSize = Math.Max(result.MaxStackSize, result.Types.Count);

            return new RawCode(result);
        }

        IR.Constructor ICompileIRObject<IR.Constructor, IR.Class>.CompileIRObject(Compiler.Compiler compiler, IR.Class? owner)
        {
            IR ??= new(Name)
            {
                Method = new(compiler.RuntimeModule.TypeSystem.Void)
            };

            if (owner is not null && !state.Get(BuildState.Owner))
            {
                owner.Constructors.Add(IR);
                owner.Methods.Add(IR.Method);

                state.Set(BuildState.Owner);
            }

            if (!state.Get(BuildState.Signature))
            {
                foreach (var arg in Signature.Args)
                    IR.Method.Signature.Args.Parameters.Add(compiler.CompileIRObject<IR.Parameter, IR.Signature>(arg, IR.Method.Signature));

                if (Signature.VarArgs is not null)
                    IR.Method.Signature.Args.Var = compiler.CompileIRObject<IR.Parameter, IR.Signature>(Signature.VarArgs, IR.Method.Signature);

                foreach (var kwArg in Signature.KwArgs)
                    IR.Method.Signature.KwArgs.Parameters.Add(compiler.CompileIRObject<IR.Parameter, IR.Signature>(kwArg, IR.Method.Signature));

                if (Signature.VarKwArgs is not null)
                    IR.Method.Signature.KwArgs.Var = compiler.CompileIRObject<IR.Parameter, IR.Signature>(Signature.VarKwArgs, IR.Method.Signature);

                state.Set(BuildState.Signature);
            }

            if (Body is not null && !state.Get(BuildState.Body))
            {
                IR.Method.Body.Instructions.AddRange(compiler.CompileIRCode(Body).Instructions);

                state.Set(BuildState.Body);
            }

            return IR;
        }
    }
}
