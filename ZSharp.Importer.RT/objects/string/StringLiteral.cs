using Value = string;

namespace ZSharp.Importer.RT.Objects
{
    internal sealed partial class StringLiteral(Value value, IR.IType type)
        : CompilerObject
    {
        private readonly Value value = value;
        private readonly IR.IType type = type;
    }
}
