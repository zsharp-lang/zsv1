namespace ZSharp.VM
{
    public sealed class ZSSignature(IR.Signature signature) 
        //: ZSObject(TypeSystem.Any)
        //, IIRObject
    {
        public IR.Signature Signature { get; } = signature;

        public IR.IRObject IR => Signature;

        public List<ZSObject> Args { get; } = [];

        public ZSObject? VarArgs { get; set; }

        public Dictionary<string, ZSObject> KwArgs { get; } = [];

        public ZSObject? VarKwArgs { get; set; }
    }
}
