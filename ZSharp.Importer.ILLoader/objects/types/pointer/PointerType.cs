using ZSharp.IR;

namespace ZSharp.Importer.ILLoader.Objects
{
    public sealed class PointerType(OOPType type)
        : CompilerObject
    {
        private readonly OOPType type = type;
    }
}
