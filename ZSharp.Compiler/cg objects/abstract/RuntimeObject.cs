using ZSharp.VM;

namespace ZSharp.Compiler
{
    internal sealed class RuntimeObject(ZSObject @object)
        : CGObject
    {
        public ZSObject Object { get; } = @object;
    }
}
