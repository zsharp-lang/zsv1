namespace ZSharp.CTRuntime
{
    internal sealed class CGFunctionType(VM.ZSSignature signature)
        : ZSValue(VM.TypeSystem.Type)
        , IRTCallable
        , ICTCallable
    {
        public VM.ZSSignature Signature { get; } = signature;

        public RTType Call(ZSCompiler compiler, RTType callable, Argument<RTType>[] arguments)
        {
            throw new NotImplementedException();
            //var fn = callable.Type;

            //Code code = new(0, [], compiler.Interpreter.LoadIR(Signature.ReturnType as IR.IRObject));

            //foreach (var arg in arguments)
            //{
            //    var argCode = arg.
            //}
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

            var result = compiler.Interpreter.Call(function, argsOrder);
            if (result is not VM.ZSHandle<Code> codeHandle)
                throw new();

            var code = compiler.Interpreter.Assemble(codeHandle.Object.Instructions);
            return compiler.Interpreter.Evaluate(code, codeHandle.Object.StackSize);
        }
    }
}
