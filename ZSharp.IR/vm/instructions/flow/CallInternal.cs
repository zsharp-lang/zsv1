namespace ZSharp.IR.VM
{
    public sealed class CallInternal(ICallable function) 
        : Instruction
        , IHasOperand<ICallable>
    {
        public ICallable Callable { get; set; } = function;

        ICallable IHasOperand<ICallable>.Operand => Callable;
    }
}
