using ZSharp.RAST;

namespace ZSharp.CTRuntime
{
    internal sealed class FunctionCompiler(ZSCompiler compiler) : CompilerBase(compiler)
    {
        private readonly SignatureCompiler signatureCompiler = new(compiler);
        private readonly FunctionBodyCompiler functionBodyCompiler = new(compiler);

        public IR.Function Compile(RFunction function)
        {
            var functionBinding = Context.GetCT<RTFunction>(function);

            var signature = signatureCompiler.Compile(function.Signature, function.ReturnType);

            if (functionBinding is null)
                using (RT.UseScope(function))
                    functionBinding = Context.Set(
                        function.Id,
                        new RTFunction(new(signature)
                        {
                            Name = function.Name
                        }, Compiler.Interpreter.LoadIR<VM.ZSSignature>(signature))
                    );
            else
            {
                functionBinding.IR.Signature = signature;
                functionBinding.Type = new RTFunctionType(Compiler.Interpreter.LoadIR<VM.ZSSignature>(signature));
            }

            var functionIR = functionBinding.IR;

            using (RT.UseScope(function))
                functionBodyCompiler.Compile(function, functionIR);

            return functionIR;
        }
    }
}
