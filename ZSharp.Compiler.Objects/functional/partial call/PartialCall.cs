using CommonZ.Utils;
using ZSharp.Compiler;

namespace ZSharp.Objects
{
    public sealed class PartialCall(CompilerObject target)
        : CompilerObject
        , ICTCallable
    {
        public CompilerObject Target { get; set; } = target;

        public Argument[] Arguments { get; set; } = [];

        public required ISignature Signature { get; set; }

        CompilerObject ICTCallable.Call(Compiler.Compiler compiler, Argument[] arguments)
        {
            return compiler.Call(Target, [
                .. Arguments,
                .. arguments
            ]);
        }

        public static PartialCall CreateFrom(Compiler.Compiler compiler, CompilerObject target, ISignature signature, Argument[] arguments)
            => new(target)
            {
                Signature = ApplyArguments(compiler, signature, arguments),
                Arguments = arguments
            };

        private static Signature ApplyArguments(Compiler.Compiler compiler, ISignature origin, Argument[] arguments)
        {
            var (args, kwArgs) = Utils.SplitArguments(arguments);

            return ApplyArguments(compiler, origin, args, kwArgs);
        }

        private static Signature ApplyArguments(Compiler.Compiler compiler, ISignature origin, Collection<CompilerObject> args, Mapping<string, CompilerObject> kwArgs)
        {
            if (args.Count > origin.Args.Count() && origin.VarArgs is null)
                throw new ArgumentsCountMismatchException();

            if (kwArgs.Count > origin.KwArgs.Count() && origin.VarKwArgs is null)
                throw new ArgumentsCountMismatchException();

            List<CompilerObject> varArgs = [];
            if (args.Count > origin.Args.Count())
                varArgs.AddRange(args.Skip(origin.Args.Count()));
            args = [.. args.Take(origin.Args.Count())];

            Signature result = new()
            {
                ReturnType = origin.ReturnType,
            };

            foreach (var (arg, param) in args.Zip(origin.Args))
                if (!compiler.TypeSystem.IsTyped(arg, out var argumentType))
                    throw new ArgumentException("Argument is not typed.");
                else if (!compiler.TypeSystem.IsAssignableTo(argumentType, param.Type ?? throw new()))
                    throw new Compiler.InvalidCastException(arg, param.Type);
            
            foreach (var param in origin.Args.Skip(args.Count))
                result.Args.Add(new(param.Name)
                {
                    Default = param.Default,
                    //Initializer = param.Initializer,
                    Type = param.Type,
                });

            foreach (var varArg in varArgs)
                if (
                    !compiler.TypeSystem.IsTyped(varArg, out var argumentType) ||
                    !compiler.TypeSystem.IsAssignableTo(argumentType, origin.VarArgs!.Type ?? throw new())
                )
                    throw new Compiler.InvalidCastException(varArg, origin.VarArgs!.Type!);

            if (origin.VarArgs is not null)
                result.VarArgs = new(origin.VarArgs.Name)
                {
                    Type = origin.VarArgs.Type,
                };

            foreach (var param in origin.KwArgs)
            {
                if (!kwArgs.Remove(param.Name, out var arg))
                {
                    result.KwArgs.Add(new(param.Name)
                    {
                        Default = param.Default,
                        //Initializer = param.Initializer,
                        Type = param.Type,
                    });
                    continue;
                }

                if (
                    !compiler.TypeSystem.IsTyped(arg, out var argumentType) ||
                    !compiler.TypeSystem.IsAssignableTo(argumentType, param.Type ?? throw new())
                )
                    throw new Compiler.InvalidCastException(arg, param.Type!);
            }

            foreach (var varKwArg in kwArgs.Values)
                if (
                    !compiler.TypeSystem.IsTyped(varKwArg, out var argumentType) ||
                    !compiler.TypeSystem.IsAssignableTo(argumentType, origin.VarKwArgs!.Type ?? throw new())
                )
                    throw new Compiler.InvalidCastException(varKwArg, origin.VarKwArgs!.Type!);

            if (origin.VarKwArgs is not null)
                result.VarKwArgs = new(origin.VarKwArgs.Name)
                {
                    Type = origin.VarKwArgs.Type,
                };

            return result;
        }
    }
}
