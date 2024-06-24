namespace ZSharp.VM
{
    public sealed class ZSFunction(ZSSignature signature, Instruction[] code) 
        : ZSObject(new Types.FunctionType(signature))
    {
        public Instruction[] Code { get; set; } = code;

        public int StackSize { get; set; } = 0;

        public int LocalCount { get; set; } = 0;
    }
}
