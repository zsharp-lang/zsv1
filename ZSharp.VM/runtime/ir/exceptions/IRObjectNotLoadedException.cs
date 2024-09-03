namespace ZSharp.VM
{
    public class IRObjectNotLoadedException(IR.IRObject ir) : ZSRuntimeException
    {
        public IR.IRObject IR { get; } = ir;
    }

    public class IRObjectNotLoadedException<T>(T ir) : IRObjectNotLoadedException(ir)
        where T : IR.IRObject
    {
        public new T IR => (T)base.IR;
    }
}
