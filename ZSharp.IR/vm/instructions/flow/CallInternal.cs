namespace ZSharp.IR.VM
{
    public sealed class CallInternal(Function function) 
        : Instruction
        , IHasOperand<Function>
    {
        public Function Function { get; set; } = function;

        Function IHasOperand<Function>.Operand => Function;
    }
}
