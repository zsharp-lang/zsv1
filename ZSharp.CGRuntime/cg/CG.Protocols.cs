namespace ZSharp.CGRuntime
{
    public static partial class CG
    {
        public static HLVM.Assign Assign()
            => new();

        public static HLVM.Argument Argument(string? name = null)
            => new(name);

        public static HLVM.Call Call(int argumentCount)
            => new(argumentCount);

        public static HLVM.Cast Cast()
            => new();
    }
}
