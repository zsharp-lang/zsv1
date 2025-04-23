using ZSharp.Compiler;

using Args = CommonZ.Utils.Collection<ZSharp.Objects.CompilerObject>;
using KwArgs = CommonZ.Utils.Mapping<string, ZSharp.Objects.CompilerObject>;


namespace ZSharp.Objects
{
    public sealed class Constructor(string? name)
        : CompilerObject
        , ICTCallable
        , ICompileIRObject<IR.Constructor, IR.Class>
        , IReferencable<ConstructorReference>
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

        public CompilerObject? Owner { get; set; }

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
                var type = Owner ?? Signature.Args[0].Type;

                hasReturn = true;

                invocationInstruction = new IR.VM.CreateInstance(new IR.ConstructorReference(IR!)
                {
                    OwningType = (IR?.Method.Owner is null 
                        ? IR!.Method.Signature.Args.Parameters[0].Type as IR.OOPTypeReference
                        : new IR.ClassReference(IR!.Method.Owner as IR.Class ?? throw new()))
                        ?? throw new()
                });
                args.Insert(0, new RawCode(new()
                {
                    Types = [type]
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
                result.Types.Add(Owner ?? Signature.Args[0].Type);

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

            if (!state[BuildState.Signature])
            {
                state[BuildState.Signature] = true;

                compiler.CompileIRObject<IR.Signature, IR.Signature>(Signature, IR.Method.Signature);
            }

            if (Body is not null && !state.Get(BuildState.Body))
            {
                IR.Method.Body.Instructions.AddRange(compiler.CompileIRCode(Body).Instructions);

                state.Set(BuildState.Body);
            }

            return IR;
        }

        ConstructorReference IReferencable<ConstructorReference>.CreateReference(Referencing @ref, ReferenceContext context)
        {
            return new(this, context);
        }
    }
}
