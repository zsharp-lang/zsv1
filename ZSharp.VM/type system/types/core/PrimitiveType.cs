namespace ZSharp.VM.Types
{
    public class PrimitiveType(IR.Class @class, ZSObject type) 
        : ZSIRObject<IR.Class>(@class, 0, type)
    {
        public string Name => IR?.Name ?? "unknown type";

        public override string ToString()
        {
            return Name;
        }
    }
}
