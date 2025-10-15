using ZSharp.Compiler;
using ZSharp.Importer.ILLoader;

namespace ZSharp.CLI
{
    [ModuleScope]
    public static class StandardTypes
    {
        [Alias("Void")]
        public static CompilerObject VoidType = null!;
    }
}
