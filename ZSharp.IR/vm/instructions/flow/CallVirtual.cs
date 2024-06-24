namespace ZSharp.IR.VM
{
    public sealed class CallVirtual(Method method) : Instruction
    {
        public Method Method { get; set; } = method;
    }
}
