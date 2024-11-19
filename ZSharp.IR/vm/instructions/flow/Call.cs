namespace ZSharp.IR.VM
{
    public sealed class Call(ICallable callable) 
        : Instruction
        , IHasOperand<ICallable>
    {
        public ICallable Callable { get; set; } = callable;

        ICallable IHasOperand<ICallable>.Operand => Callable;
    }
}
