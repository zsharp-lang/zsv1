using ZSharp.Compiler;
using ZSharp.IR;

namespace ZSharp.CGObjects
{
    public abstract class Literal(object? value)
        : CGObject
        , ICTReadable
    {
        public abstract IType Type { get; }

        public object? Value { get; } = value;

        public abstract Code Read(ICompiler compiler);
    }

    public abstract class Literal<T>(T value)
        : Literal(value)
    {
        public new T Value => (T)base.Value!;
    }
}
