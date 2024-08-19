namespace ZSharp.CGRuntime
{
    public static partial class CG
    {
        public static HLVM.Binding Get(string name)
            => HLVM.Binding.Get(name);

        public static HLVM.Binding Set(string name)
            => HLVM.Binding.Set(name);

        public static HLVM.Binding Del(string name)
            => HLVM.Binding.Del(name);
    }
}
