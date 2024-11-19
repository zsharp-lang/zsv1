using CommonZ.Utils;

namespace ZSharp.Compiler
{
    public sealed class Code
    {
        public IRCode Instructions { get; init; } = [];

        public int MaxStackSize { get; set; }

        public Collection<CGObject> Types { get; init; } = [];

        public bool IsVoid => Types.Count == 0;

        public bool IsValue => Types.Count == 1;

        public bool IsArray => Types.Count > 1;

        public Code() { }

        public Code(IEnumerable<IR.VM.Instruction> instructions)
        {
            Instructions = [.. instructions];
        }

        public void RequireVoidType()
        {
            if (IsVoid) return;
            throw new InvalidOperationException();
        }

        public CGObject RequireValueType()
            => IsValue ? Types[0] : throw new InvalidOperationException();

        public void Append(Code other)
        {
            Instructions.AddRange(other.Instructions);
            Types.AddRange(other.Types);
            MaxStackSize = Math.Max(
                MaxStackSize,
                Types.Count + other.MaxStackSize
            );
        }

        public static readonly Code Empty = new()
        {
            Instructions = Collection<IR.VM.Instruction>.Empty,
            MaxStackSize = 0,
            Types = Collection<CGObject>.Empty,
        };
    }
}
