namespace ZSharp.VM
{
    public delegate ZSObject? ZSInternalFunctionDelegate(ZSObject[] args);

    public sealed class ZSInternalFunction
        (ZSInternalFunctionDelegate function, int argumentCount, ZSObject type)
        : ZSObject(type)
    {
        public int ArgumentCount { get; } = argumentCount;

        public ZSInternalFunctionDelegate Function { get; } = function;

        public ZSObject? Call(params ZSObject[] args) => Function(args);
    }
}
