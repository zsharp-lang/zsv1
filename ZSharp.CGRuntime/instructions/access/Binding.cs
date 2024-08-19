namespace ZSharp.CGRuntime.HLVM
{
    public sealed class Binding(string name, AccessMode accessMode) : Instruction
    {
        public string Name { get; set; } = name;

        public AccessMode AccessMode { get; set; } = accessMode;

        public static Binding Get(string name)
            => new(name, AccessMode.Get);

        public static Binding Set(string name)
            => new(name, AccessMode.Set);

        public static Binding Del(string name)
            => new(name, AccessMode.Del);
    }
}
