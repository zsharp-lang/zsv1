using ZSharp.Compiler;

namespace ZSharp.Objects
{
    public abstract class Literal(object? value)
        : CompilerObject
        , ICTReadable
    {
        public abstract CompilerObject Type { get; }

        public object? Value { get; } = value;

        public abstract IRCode Read(Compiler.Compiler compiler);
    }

    public abstract class Literal<T>(T value)
        : Literal(value)
    {
        public new T Value => (T)base.Value!;
    }
}
