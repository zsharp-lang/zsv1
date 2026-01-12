using ZSharp.IR;

namespace ZSharp.Importer.ILLoader.Objects
{
    public sealed class ReferenceType(TypeDefinition type)
        : CompilerObject
    {
        private readonly TypeDefinition type = type;
    }
}
