namespace ZSharp.VM
{
    public abstract class ZSIRObject(IR.IRObject ir, int fieldCount, ZSObject type)
        : ZSStruct(fieldCount, type)
    {
        public IR.IRObject IR { get; } = ir;
    }

    public abstract class ZSIRObject<T>(T ir, int fieldCount, ZSObject type)
        : ZSIRObject(ir, fieldCount, type)
        where T : IR.IRObject
    {
        public new T IR => (T)base.IR;
    }
}
