using ZSharp.Compiler;

namespace ZSharp.CGObjects
{
    public sealed class SimpleFunctionOverloadGroup(string name)
        : CGObject
        , ICTCallable
    {
        public string Name { get; set; } = name;

        public List<RTFunction> Overloads { get; } = [];

        public CGObject Call(Compiler.Compiler compiler, Argument[] arguments)
        {
            var (args, kwargs) = Utils.SplitArguments(arguments);
            
            CGObject? Match(RTFunction overload)
            {
                if (args.Count != overload.Signature.Args.Count) return null;
                if (kwargs.Count != overload.Signature.KwArgs.Count) return null;

                foreach (var (arg, param) in args.Zip(overload.Signature.Args))
                {
                    var code = compiler.CompileIRCode(arg);

                    if (compiler.CompileIRType(code.RequireValueType()) != compiler.CompileIRType(param.Type)) return null;
                }

                foreach (var param in overload.Signature.KwArgs)
                {
                    if (!kwargs.TryGetValue(param.Name, out var arg)) return null;

                    var code = compiler.CompileIRCode(arg);

                    if (compiler.CompileIRType(code.RequireValueType()) != compiler.CompileIRType(param.Type)) return null;
                }

                return (overload as ICTCallable).Call(compiler, arguments); // TODO: overloading protocol?????
            }

            var matchingOverloads = Overloads.Select(Match).Where(c => c is not null).ToList()!;

            if (matchingOverloads.Count != 1) throw new();

            return matchingOverloads[0]!;
        }
    }
}
