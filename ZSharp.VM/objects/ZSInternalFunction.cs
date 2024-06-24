namespace ZSharp.VM
{
    public delegate ZSObject? ZSInternalFunctionDelegate(ZSObject[] args);

    public sealed class ZSInternalFunction
        (ZSInternalFunctionDelegate function, ZSObject type = null!) 
        : ZSObject(type)
    {
        public ZSInternalFunctionDelegate Function { get; } = function;

        public ZSObject? Call(params ZSObject[] args) => Function(args);
    }
}
