namespace ZSharp.VM
{
    public sealed class ZSSignature(IR.Signature signature) 
        : ZSObject(TypeSystem.Any)
        , IIRObject
    {
        public IR.Signature Signature { get; } = signature;

        public IR.IRObject IR => Signature;

        public List<ZSObject> Args { get; } = [];

        public ZSObject? VarArgs { get; set; }

        public Dictionary<string, ZSObject> KwArgs { get; } = [];

        public ZSObject? VarKwArgs { get; set; }

        public ZSObject[] Match(Interpreter interpreter, List<ZSObject> args, Dictionary<string, ZSObject> kwArgs)
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
            List<ZSObject> result = [];

            for (int i = 0; i < Args.Count; i++)
            {
                var arg = args[i];
                var param = Args[i];

                if (!interpreter.TypeSystem.IsAssignableTo(arg.Type, param))
                    //result.TypeMismatch(arg.Type, param, i);
                    throw new($"Type mismatch: cannot assign type {arg.Type} to {param} in parameter {i}");
                else
                    result.Add(arg);
            }

            foreach (var (name, param) in KwArgs)
            {
                var arg = kwArgs[name];

                if (!interpreter.TypeSystem.IsAssignableTo(arg.Type, param))
                    //result.TypeMismatch(arg.Type, param, i);
                    throw new($"Type mismatch: cannot assign type {arg.Type} to {param} in parameter {name}");
                else
                    result.Add(arg);
            }

            return result.ToArray();
        }
    }
}
