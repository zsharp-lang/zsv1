using CommonZ.Utils;
using ZSharp.IR;

namespace ZSharp.Importer.ILLoader
{
    internal sealed class ConstructedTypeDefinition
        : ConstructedType<TypeDefinition>
    {
        public required Collection<IType> Arguments { get; init; }

        public required TypeDefinition Definition { get; init; }

        public TypeReference? OwningType { get; init; }
    }
}
