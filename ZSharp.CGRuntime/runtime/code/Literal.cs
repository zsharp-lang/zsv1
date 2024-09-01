using ZSharp.RAST;

namespace ZSharp.CGRuntime.LLVM
{
    internal readonly struct Literal(object value, RLiteralType type)
    {
        public object Value { get; } = value;

        public RLiteralType Type { get; } = type;
    }
}
