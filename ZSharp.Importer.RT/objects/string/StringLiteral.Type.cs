using ZSharp.Compiler;

namespace ZSharp.Importer.RT.Objects
{
    partial class StringLiteral
        : ITyped
    {
        CompilerObject ITyped.Type => type;
    }
}
