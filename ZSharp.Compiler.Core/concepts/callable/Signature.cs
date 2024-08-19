using CommonZ.Utils;

namespace ZSharp.CTRuntime
{
    public sealed class Signature(IR.Signature signature)
        : IBinding
    {
        private Collection<Parameter>? _args;
        private Mapping<string, Parameter>? _kwArgs;

        public IR.Signature IR { get; } = signature;

        public List<Parameter> Args
        {
            get
            {
                if (_args is not null)
                    return _args;

                Interlocked.CompareExchange(ref _args, new(), null);
                return _args;
            }
        }

        public Dictionary<string, Parameter> KwArgs
        {
            get
            {
                if (_kwArgs is not null)
                    return _kwArgs;

                Interlocked.CompareExchange(ref _kwArgs, new(), null);
                return _kwArgs;
            }
        }

        public Parameter? VarArgs { get; set; }

        public Parameter? VarKwArgs { get; set; }

        public CTObject[] Match(ZSCompiler compiler, List<CTObject> args, Dictionary<string, CTObject> kwArgs)
        {
            if (args.Count > Args.Count && VarArgs is null)
                //return MatchResult.TooManyArguments();
                throw new("Too many arguments");
            else if (args.Count < Args.Count)
                throw new NotImplementedException("Default values for positional arguments is not implemented");

            if (kwArgs.Count > KwArgs.Count && VarKwArgs is null)
                //return MatchResult.TooManyKeywordArguments();
                throw new("Too many keyword arguments");
            else if (kwArgs.Count < KwArgs.Count)
                throw new NotImplementedException("Default values for keyword arguments is not implemented");

            //MatchResult result = new();
            List<CTObject> result = [];

            for (int i = 0; i < Args.Count; i++)
            {
                var arg = args[i];
                var param = Args[i];

                if (!compiler.Interpreter.TypeSystem.IsAssignableTo(arg.Type, param.Type))
                    //result.TypeMismatch(arg.Type, param, i);
                    throw new($"Type mismatch: cannot assign type {arg.Type} to {param} in parameter {i}");
                else
                    result.Add(arg);
            }

            foreach (var (name, param) in KwArgs)
            {
                var arg = kwArgs[name];

                if (!compiler.Interpreter.TypeSystem.IsAssignableTo(arg.Type, param.Type))
                    //result.TypeMismatch(arg.Type, param, i);
                    throw new($"Type mismatch: cannot assign type {arg.Type} to {param} in parameter {name}");
                else
                    result.Add(arg);
            }

            return [.. result];
        }
    }
}
