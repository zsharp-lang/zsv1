using System.Diagnostics.CodeAnalysis;

namespace ZSharp.Compiler
{
    public sealed class CastResult
    {
        public required CompilerObject Cast { get; set; }

        public ZSharp.IR.VM.Instruction? OnFail { get; init; } = null;

        [MemberNotNullWhen(true, nameof(OnFail))]
        public bool CanFail => OnFail is not null;
    }
}
