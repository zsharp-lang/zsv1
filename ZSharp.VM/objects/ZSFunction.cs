namespace ZSharp.VM
{
    public sealed class ZSFunction(Instruction[] code) 
        : ZSObject(TypeSystem.Function)
    {
        public Instruction[] Code { get; set; } = code;

        public int StackSize { get; set; } = 0;

        public int LocalCount { get; set; } = 0;
    }
}
