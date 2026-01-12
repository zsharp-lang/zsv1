using ZSharp.IR;

namespace ZSharp.Importer.ILLoader.Objects
{
    public sealed class ArrayType(TypeDefinition type)
        : CompilerObject
    {
        private readonly TypeDefinition type = type;
    }
}
