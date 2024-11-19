using ZSharp.Compiler;

namespace ZSharp.CGObjects
{
    public abstract class Literal(object? value)
        : CGObject
        , ICTReadable
    {
        public abstract CGObject Type { get; }

        public object? Value { get; } = value;

        public abstract Code Read(Compiler.Compiler compiler);
    }

    public abstract class Literal<T>(T value)
        : Literal(value)
    {
        public new T Value => (T)base.Value!;
    }
}
