namespace ZSharp.CGRuntime.HLVM
{
    public sealed class Index(int argumentCount, AccessMode accessMode) : Instruction
    {
        public int ArgumentCount { get; set; } = argumentCount;

        public AccessMode AccessMode { get; set; } = accessMode;

        public static Index Get(int argumentCount)
            => new(argumentCount, AccessMode.Get);

        public static Index Set(int argumentCount)
            => new(argumentCount, AccessMode.Set);

        public static Index Del(int argumentCount)
            => new(argumentCount, AccessMode.Del);
    }
}
