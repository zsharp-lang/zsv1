namespace ZSharp.IR
{
    [Flags]
    public enum FunctionAttributes
    {
        None = 0,
        
        InstanceMethod = 1,
        VirtualMethod = 2,
        ClassMethod = 4,
        StaticMethod = 8,
    }
}
