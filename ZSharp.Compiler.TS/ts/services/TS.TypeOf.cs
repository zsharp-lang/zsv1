using System.Diagnostics.CodeAnalysis;

namespace ZSharp.Compiler
{
    public delegate IResult TypeOf(CompilerObject @object);

    partial class TS
    {
        public TypeOf TypeOf { get; set; } = Dispatcher.TypeOf;

        public bool IsTyped(CompilerObject @object, [NotNullWhen(true)] out CompilerObject? result)
            => TypeOf(@object).Ok(out result);
    }
}
