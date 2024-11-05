namespace ZSharp.VM
{
    public sealed class ZSBoolean(bool value, ZSObject type)
        : ZSObject(type)
    {
        public bool Value { get; } = value;

        public override string ToString()
            => Value.ToString();
    }
}
