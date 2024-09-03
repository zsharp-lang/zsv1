namespace ZSharp.CGRuntime
{
    public static partial class CG
    {
        public static HLVM.Get Get(string name)
            => new(name);

        public static HLVM.Enter Enter()
            => new();

        public static HLVM.Leave Leave()
            => new();
    }
}
