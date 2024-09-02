using ZSharp.VM;

namespace ZSharp.Compiler
{
    internal partial class IRGenerator
    {
        private readonly Runtime rtRuntime;

        public Runtime RT => rtRuntime;

        public ZSObject EvaluateRT(Code code)
        {
            code.RequireValueType();
            var compiledCode = rtRuntime.Assemble(code.Instructions);
            compiledCode = new(compiledCode.Instructions, 1);
            return rtRuntime.EvaluateInNewFrame(compiledCode);
        }
    }
}
