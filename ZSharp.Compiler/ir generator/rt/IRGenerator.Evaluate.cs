using ZSharp.VM;

namespace ZSharp.Compiler
{
    internal partial class IRGenerator
    {
        public ZSObject EvaluateRT(Code code)
        {
            code.RequireValueType();
            var compiledCode = RT.Assemble(code.Instructions);
            compiledCode = new(compiledCode.Instructions, code.MaxStackSize);
            return RT.EvaluateInNewFrame(compiledCode);
        }
    }
}
