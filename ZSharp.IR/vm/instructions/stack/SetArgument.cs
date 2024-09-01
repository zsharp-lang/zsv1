namespace ZSharp.IR.VM
{
    public sealed class SetArgument(Parameter argument) 
        : Instruction
        , IHasOperand<Parameter>
    {
        public Parameter Argument { get; set; } = argument;

        Parameter IHasOperand<Parameter>.Operand => Argument;
    }
}
