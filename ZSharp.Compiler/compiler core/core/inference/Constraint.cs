namespace ZSharp.Compiler
{
    public abstract class Constraint
    {
        public CompilerObject Target { get; internal set; } = null!;
    }
}
