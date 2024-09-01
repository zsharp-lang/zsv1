namespace ZSharp.IR.VM
{
    public sealed class Call(Function function) 
        : Instruction
        , IHasOperand<Function>
    {
        public Function Function { get; set; } = function;

        Function IHasOperand<Function>.Operand => Function;
    }
}
