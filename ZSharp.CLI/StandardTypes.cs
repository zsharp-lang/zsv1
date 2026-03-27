using ZSharp.Compiler;
using ZSharp.Importer.ILLoader;

namespace ZSharp.CLI
{
    [ModuleScope]
    [ImportType(typeof(Test), Namespace = "")]
    public static class StandardTypes
    {
        [Alias("Void")]
        public static CompilerObject VoidType = null!;

        [Alias("Object")]
        public static CompilerObject ObjectType = null!;
    }

    public class Test(string s)
    {
        public override string ToString()
            => $"Test: {s}";
    }
}
