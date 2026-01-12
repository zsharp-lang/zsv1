using Value = string;

namespace ZSharp.Importer.RT.Objects
{
    internal sealed partial class StringLiteral(Value value, CompilerObject type)
        : CompilerObject
    {
        private readonly Value value = value;
        private readonly CompilerObject type = type;
    }
}
