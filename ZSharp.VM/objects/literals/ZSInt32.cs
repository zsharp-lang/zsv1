namespace ZSharp.VM
{
    public sealed class ZSInt32(int value, ZSObject type)
        : ZSObject(type)
    {
        public int Value { get; } = value;
    }
}
