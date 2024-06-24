using CommonZ.Utils;

namespace ZSharp.CTRuntime
{
    public sealed class RTFunctionType(VM.ZSSignature signature) 
        : ZSValue(VM.TypeSystem.Type)
        , IRTCallable
        , ICTCallable
    {
        public VM.ZSSignature Signature { get; } = signature;

        public RTType Call(ZSCompiler compiler, RTType callable, Argument<RTType>[] arguments)
        {
            List<Code> args = [];
            Dictionary<string, Code> kwArgs = [];

            foreach (var arg in arguments)
            {
                var value = arg.Binding.Read(compiler);

                if (arg.Name is null)
                    args.Add(value);
                else
                    kwArgs[arg.Name] = value;
            }

            CodeCombiner argsCC = new(), kwargsCC = new();
            CodeCombiner varArgsCC = new(), varKwArgsCC = new();

            var exit = new Exception("NoOverload");

            if (args.Count > Signature.Args.Count)
            {
                if (Signature.VarArgs is null) throw exit;

                var varArgsParam = Signature.VarArgs;

                for (int i = Signature.Args.Count; i < args.Count; ++i)
                {
                    var code = args[i];
                    if (!compiler.Interpreter.TypeSystem.IsAssignableTo(code.Type, varArgsParam))
                        throw exit;
                    varArgsCC.Add(args[i]);
                }
            }

            if (kwArgs.Count > Signature.KwArgs.Count && Signature.VarKwArgs is null)
                throw exit;

            for (int i = 0; i < Signature.Args.Count; ++i)
            {
                var code = args[i];
                if (!compiler.Interpreter.TypeSystem.IsAssignableTo(code.Type, Signature.Args[i]))
                    throw exit;
                argsCC.Add(args[i]);
            }

            if (Signature.KwArgs.Count > 0)
            {
                var varKwArgsParam = Signature.VarKwArgs;

                foreach (var (name, param) in Signature.KwArgs)
                {
                    if (!kwArgs.TryGetValue(name, out var code))
                    {
                        // try default value

                        // for now, suppose that we don't have default values
                        throw exit;
                    }

                    if (!compiler.Interpreter.TypeSystem.IsAssignableTo(code.Type, param))
                        throw exit;

                    kwargsCC.Add(code);

                    kwArgs.Remove(name);
                }

                if (varKwArgsParam is null && kwArgs.Count > 0)
                    throw exit;

                foreach (var (name, code) in kwArgs)
                {
                    if (!compiler.Interpreter.TypeSystem.IsAssignableTo(code.Type, varKwArgsParam))
                        throw exit;

                    varKwArgsCC.Add(code);
                }
            }

            CodeCombiner argumentsCode = new();

            argumentsCode.Add(argsCC.Create());

            if (varArgsCC.Code.Count > 0)
            {
                throw new NotImplementedException();
                //argumentsCode.Add(varArgs.Create());
                //argumentsCode.Add(); // create a new tuple or something
            }

            argumentsCode.Add(kwargsCC.Create());

            if (varKwArgsCC.Code.Count > 0)
            {
                throw new NotImplementedException();
                //argumentsCode.Add(varKwArgs.Create());
                //argumentsCode.Add(); // create a new mapping or something
            }

            argumentsCode.Add(callable);
            argumentsCode.Add(new IR.VM.CallIndirect(Signature.Signature));
            argumentsCode.Types.Push(compiler.Interpreter.LoadType(Signature.Signature.ReturnType));

            return argumentsCode.Create();
        }

        public CTType Call(ZSCompiler compiler, CTType callable, Argument<CTType>[] arguments)
        {
            if (callable is not VM.ZSFunction function)
                throw new();

            List<ZSValue> args = [];
            Dictionary<string, ZSValue> kwArgs = [];

            foreach (var arg in arguments)
            {
                var value = arg.Binding.Read(compiler);

                if (arg.Name is null)
                    args.Add(value);
                else
                    kwArgs[arg.Name] = value;
            }

            var argsOrder = Signature.Match(compiler.Interpreter, args, kwArgs);

            return compiler.Interpreter.Call(function, argsOrder);
        }
    }
}
