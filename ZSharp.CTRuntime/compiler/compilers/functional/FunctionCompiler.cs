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
            if (functionBinding is null)
            {
                using (RT.UseScope(function))
                {
                    var signature = signatureCompiler.Compile(function.Signature);
                    functionBinding = Context.Set(
                        function.Id,
                        new RTFunction(new(signature)
                        {
                            Name = function.Name
                        }, Compiler.Interpreter.LoadIR<VM.ZSSignature>(signature))
                    );
                }
            }

            var functionIR = functionBinding.IR;

            using (RT.UseScope(function))
                functionBodyCompiler.Compile(function, functionIR);

            return functionIR;
        }
    }
}
