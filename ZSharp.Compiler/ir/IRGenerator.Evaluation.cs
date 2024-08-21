using CommonZ.Utils;

namespace ZSharp.Compiler
{
    internal partial class IRGenerator
        //: ICodeInjector
        //, ICompiler
    {
        private IRType EvaluateType(Collection<CGRuntime.HLVM.Instruction> instructions)
            => EvaluateType(runtime.Run(instructions));

        private IRType EvaluateType(IEnumerable<CGObject> objects)
            => EvaluateType(Read(objects));

        private IRType EvaluateType(Code code)
        {
            throw new NotImplementedException();
        }
    }
}
