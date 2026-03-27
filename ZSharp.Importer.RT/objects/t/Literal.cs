using Value = object;

namespace ZSharp.Importer.RT.Objects
{
    internal sealed partial class Literal(Value value, IR.IType type)
        : CompilerObject
    {
        private readonly Value value = value;
        private readonly IR.IType type = type;
    }
}
