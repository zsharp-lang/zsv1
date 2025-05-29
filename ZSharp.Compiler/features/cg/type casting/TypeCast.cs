using System.Diagnostics.CodeAnalysis;

namespace ZSharp.Compiler
{
    public sealed class TypeCast
    {
        public required CompilerObject Cast { get; set; }

        public ZSharp.IR.VM.Instruction OnCast { get; init; } = new ZSharp.IR.VM.Nop();

        public ZSharp.IR.VM.Instruction? OnFail { get; init; } = null;

        [MemberNotNullWhen(true, nameof(OnFail))]
        public bool CanFail => OnFail is not null;
    }
}
