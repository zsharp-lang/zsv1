using CommonZ.Utils;

namespace ZSharp.Objects
{
    public sealed class Implementation(
        CompilerObject @abstract,
        CompilerObject concrete
    )
    {
        public Mapping<CompilerObject, CompilerObject> Mapping { get; } = [];

        public CompilerObject Abstract { get; } = @abstract;

        public CompilerObject Concrete { get; } = concrete;
    }
}
