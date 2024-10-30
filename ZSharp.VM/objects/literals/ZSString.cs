namespace ZSharp.VM
{
    public sealed class ZSString(string value, ZSObject type) 
        : ZSObject(type)
    {
        public string Value { get; } = value;
    }
}
