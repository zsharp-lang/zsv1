using CommonZ.Utils;

namespace ZSharp.IR
{
    public sealed class FunctionGroup(params Function[] functions)
    {
        public string? Name { get; set; }

        public FunctionGroupAttributes Attributes { get; set; } = FunctionGroupAttributes.None;

        public Collection<Function> Overloads { get; private set; } = new(functions);

        public FunctionGroup? Parent { get; }
    }
}
