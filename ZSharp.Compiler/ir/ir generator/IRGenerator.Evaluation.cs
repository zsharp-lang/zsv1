using CommonZ.Utils;

namespace ZSharp.Compiler
{
    internal partial class IRGenerator
        //: ICodeInjector
        //, ICompiler
    {
        public IRType EvaluateType(Collection<CGRuntime.HLVM.Instruction> instructions)
            => EvaluateType(runtime.Run(instructions));

        public IRType EvaluateType(IEnumerable<CGObject> objects)
            => EvaluateType(Read(objects));

        public IRType EvaluateType(Code code)
        {
            throw new NotImplementedException();
        }
    }
}
