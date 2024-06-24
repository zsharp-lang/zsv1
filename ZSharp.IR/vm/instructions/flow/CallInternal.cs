namespace ZSharp.IR.VM
{
    public sealed class CallInternal(InternalFunction function) : Instruction
    {
        public InternalFunction Function { get; set; } = function;
    }
}
