namespace ZSharp.VM
{
    public sealed class ZSFloat32(float value, ZSObject type)
        : ZSObject(type)
    {
        public float Value { get; } = value;

        public override string ToString()
            => Value.ToString();
    }
}
