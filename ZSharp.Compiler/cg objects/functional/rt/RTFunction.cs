using ZSharp.Compiler;
using ZSharp.IR;

namespace ZSharp.Objects
{
    public class RTFunction(string? name)
        : Function(name)
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

        private BuildState _state = BuildState.None;

        CompilerObject ITyped.Type => throw new NotImplementedException();

        public Signature Signature { get; set; } = new();

        public CompilerObject? ReturnType { get; set; }

        public bool IsSignatureBuilt
        {
            get => _state.HasFlag(BuildState.Signature);
            set => _state = value ? _state | BuildState.Signature : _state & ~BuildState.Signature;
        }

        public bool IsBodyBuilt
        {
            get => _state.HasFlag(BuildState.Body);
            set => _state = value ? _state | BuildState.Body : _state & ~BuildState.Body;
        }

        public bool IsOwnerBuilt
        {
            get => _state.HasFlag(BuildState.Owner);
            set => _state = value ? _state | BuildState.Owner : _state & ~BuildState.Owner;
        }

        public CompilerObject Call(IRCode[] arguments)
        {
            if (IR is null)
                throw new("Function not compiled.");

            IRCode code = new();

            foreach (var argument in arguments)
                code.Append(argument);

            code.Instructions.Add(new IR.VM.Call(IR));

            throw new NotImplementedException();
            //return new Code(code);
        }

        public override CompilerObject Call(Compiler.Compiler compiler, Argument[] arguments)
        {
            var (args, kwargs) = Utils.SplitArguments(arguments);

            return Call(compiler, args, kwargs);
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
                result.Types.Add(ReturnType ?? throw new()); // TODO: WTF?????? This is here because the 
            // IR -> CG loader is not yet implemented.

            result.MaxStackSize = Math.Max(result.MaxStackSize, result.Types.Count);

            return new RawCode(result);
        }

        IRCode ICTReadable.Read(Compiler.Compiler compiler)
            => new([
                new IR.VM.GetObject(IR!)
            ])
            {
                Types = [null!], // TODO: fix type
            };

        public IR.Function CompileIRObject(Compiler.Compiler compiler, IR.Module? owner)
        {
            IR ??= new(compiler.CompileIRType(ReturnType ?? throw new()))
            {
                Name = Name
            };

            if (owner is not null && !IsOwnerBuilt)
            {
                owner.Functions.Add(IR);

                IsOwnerBuilt = true;
            }

            if (!IsSignatureBuilt)
            {
                foreach (var arg in Signature.Args)
                    IR.Signature.Args.Parameters.Add(compiler.CompileIRObject<IR.Parameter, IR.Signature>(arg, IR.Signature));

                if (Signature.VarArgs is not null)
                    IR.Signature.Args.Var = compiler.CompileIRObject<IR.Parameter, IR.Signature>(Signature.VarArgs, IR.Signature);

                foreach (var kwArg in Signature.KwArgs)
                    IR.Signature.KwArgs.Parameters.Add(compiler.CompileIRObject<IR.Parameter, IR.Signature>(kwArg, IR.Signature));

                if (Signature.VarKwArgs is not null)
                    IR.Signature.KwArgs.Var = compiler.CompileIRObject<IR.Parameter, IR.Signature>(Signature.VarKwArgs, IR.Signature);

                IsSignatureBuilt = true;
            }

            if (Body is not null && !IsBodyBuilt)
            {
                IR.Body.Instructions.AddRange(compiler.CompileIRCode(Body).Instructions);

                IsBodyBuilt = true;
            }

            // TODO: compile signature

            return IR;
        }
    }
}
