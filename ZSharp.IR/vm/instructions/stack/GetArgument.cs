namespace ZSharp.IR.VM
{
    public sealed class GetArgument(Parameter argument) : Instruction
    {
        public Parameter Argument { get; set; } = argument;
    }
}
