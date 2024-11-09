namespace ZSharp.IR
{
    [Flags]
    public enum MethodAttributes
    {
        None = 0,

        InstanceMethod = 1,
        VirtualMethod = 2,
        ClassMethod = 4,
        StaticMethod = 8,
    }
}
