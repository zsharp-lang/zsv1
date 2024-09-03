namespace ZSharp.IR.VM
{
    [Flags]
    public enum LocalAttributes
    {
        None = 0,

        ReadOnly = 1 << 0,
    }
}
