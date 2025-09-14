using CommonZ.Utils;

namespace ZSharp.Platform.Runtime
{
    partial class Runtime
    {
        public object? Evaluate(Collection<IR.VM.Instruction> code, IR.IType type)
        {
            return Loader.LoadCode(code, type).DynamicInvoke(null);
        }
    }
}
