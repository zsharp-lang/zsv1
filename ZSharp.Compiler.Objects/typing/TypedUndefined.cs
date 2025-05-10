using ZSharp.Compiler;

namespace ZSharp.Objects
{
    public sealed class TypedUndefined(IType type)
        : CompilerObject
        , ITyped
    {
        public IType Type { get; } = type;
    }
}
