namespace ZSharp.IR.VM
{
    public sealed class Call(Function function) : Instruction
    {
        public Function Function { get; set; } = function;
    }
}
