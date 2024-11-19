using ZSharp.Compiler;

namespace ZSharp.CGObjects
{
    public sealed class Method(string? name)
        : CGObject
        , IRTBoundMember
        , ICTCallable
    {
        public IR.Method? IR { get; set; }

        public string? Name { get; set; } = name;

        public Signature Signature { get; set; } = new();

        public CGObject? ReturnType { get; set; }

        public CGObject Bind(Compiler.Compiler compiler, CGObject value)
            => new BoundMethod(this, value);

        public CGObject Call(Compiler.Compiler compiler, Argument[] arguments)
        {
            var (args, kwargs) = Utils.SplitArguments(arguments);

            return Call(compiler, args, kwargs);
        }

        public CGObject Call(Compiler.Compiler compiler, Args args, KwArgs kwArgs)
        {
            // TODO: type checking (when type system is implemented)

            Code
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

            Code result = new();
            result.Append(argsCode);
            result.Append(varArgsCode); // should be empty
            result.Append(kwArgsCode);
            result.Append(varKwArgsCode); // should be empty

            result.Instructions.Add(new IR.VM.Call(IR!));

            result.Types.Clear();
            result.Types.Add(ReturnType ?? throw new());

            result.MaxStackSize = Math.Max(result.MaxStackSize, result.Types.Count);

            return new RawCode(result);
        }
    }
}
