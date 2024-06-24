namespace ZSharp.VM
{
    public sealed class ZSString(string value) : ZSObject(TypeSystem.String)
    {
        public string Value { get; } = value;
    }
}
