namespace ZSharp.VM
{
    public abstract class ZSObject(ZSObject type)
    {
        public ZSObject Type { get; internal set; } = type;
    }
}
