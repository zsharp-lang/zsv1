using CommonZ.Utils;

namespace ZSharp.Objects
{
    public sealed class Implementation(
        IAbstraction @abstract,
        CompilerObject concrete
    )
    {
        public Mapping<CompilerObject, IImplementsSpecification> Mapping { get; } = [];

        public IAbstraction Abstract { get; } = @abstract;

        public CompilerObject Concrete { get; } = concrete;
    }
}
