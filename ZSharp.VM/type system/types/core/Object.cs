namespace ZSharp.VM.Types
{
    internal sealed class Object(bool hasVirtualTable = false) 
        : PrimitiveType("Object")
    {
        public VTable? VTable { get; } = hasVirtualTable ? new() : null;
    }
}
