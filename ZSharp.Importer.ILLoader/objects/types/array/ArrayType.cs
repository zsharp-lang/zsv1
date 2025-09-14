using ZSharp.IR;

namespace ZSharp.Importer.ILLoader.Objects
{
    public sealed class ArrayType(OOPType type)
        : CompilerObject
    {
        private readonly OOPType type = type;
    }
}
