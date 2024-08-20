namespace ZSharp.CGRuntime.HLVM
{
    public sealed class Object(CGObject @object) : Instruction
    {
        public CGObject CGObject { get; } = @object;

        public override string ToString()
            => $"Object({CGObject})";
    }
}
