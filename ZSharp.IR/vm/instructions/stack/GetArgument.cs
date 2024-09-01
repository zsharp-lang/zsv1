namespace ZSharp.IR.VM
{
    public sealed class GetArgument(Parameter argument) 
        : Instruction
        , IHasOperand<Parameter>
    {
        public Parameter Argument { get; set; } = argument;

        Parameter IHasOperand<Parameter>.Operand => Argument;
    }
}
