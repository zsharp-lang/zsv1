namespace ZSharp.IR.VM
{
    public sealed class SetArgument(Parameter argument) : Instruction
    {
        public Parameter Argument { get; set; } = argument;
    }
}
