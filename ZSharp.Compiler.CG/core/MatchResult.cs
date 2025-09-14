namespace ZSharp.Compiler
{
    public sealed class MatchResult
    {
        public required CompilerObject Match { get; set; }

        public ZSharp.IR.VM.Instruction OnMatch { get; init; } = new ZSharp.IR.VM.Nop();
    }
}
