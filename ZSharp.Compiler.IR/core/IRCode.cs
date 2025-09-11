using CommonZ.Utils;
using ZSharp.IR;

namespace ZSharp.Compiler
{
    public sealed class IRCode
    {
        public Collection<ZSharp.IR.VM.Instruction> Instructions { get; init; } = [];

        public int MaxStackSize { get; set; }

        public Collection<IType> Types { get; init; } = [];

        public bool IsVoid => Types.Count == 0;

        public bool IsValue => Types.Count == 1;

        public bool IsArray => Types.Count > 1;

        public IRCode() { }

        public IRCode(IEnumerable<ZSharp.IR.VM.Instruction> instructions)
        {
            Instructions = [.. instructions];
        }

        public void RequireVoidType()
        {
            if (IsVoid) return;
            throw new InvalidOperationException();
        }

        public IType RequireValueType()
            => IsValue ? Types[0] : throw new InvalidOperationException();

        public IType RequireValueType(IType? @default)
            => IsValue ? Types[0] : (@default ?? throw new InvalidOperationException());

        public void Append(IRCode other)
        {
            Instructions.AddRange(other.Instructions);
            Types.AddRange(other.Types);
            MaxStackSize = Math.Max(
                MaxStackSize,
                Types.Count + other.MaxStackSize
            );
        }

        public static readonly IRCode Empty = new()
        {
            Instructions = [],
            MaxStackSize = 0,
            Types = [],
        };
    }
}
