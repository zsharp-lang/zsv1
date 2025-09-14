using ZSharp.IR;

namespace ZSharp.Importer.ILLoader.Objects
{
    public sealed class ReferenceType(OOPType type)
        : CompilerObject
    {
        private readonly OOPType type = type;
    }
}
