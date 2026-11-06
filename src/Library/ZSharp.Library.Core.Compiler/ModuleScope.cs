using ZSharp.Importer.ILLoader;

namespace Core.Compiler
{
    /// <summary>
    /// The toolchain objects as seen through <c>core:compiler</c>. Z# reaches them
    /// by importing that module, and C# reaches them here.
    /// </summary>
    [ModuleScope]
    public static class ModuleScope
    {
        [Alias("cg")]
        public static ZSharp.Compiler.CG CG { get; set; } = new();

        [Alias("ts")]
        public static ZSharp.Compiler.TS TS { get; set; } = new();

        [Alias("ir")]
        public static ZSharp.Compiler.IR IR { get; set; } = new();

        [Alias("hirLoader")]
        public static ZSharp.Compiler.HIRLoader HIRLoader { get; set; } = new();

        [Alias("hirCodeLoader")]
        public static ZSharp.Compiler.HIRCodeLoader HIRCodeLoader { get; set; } = new();

        [Alias("hirReifier")]
        public static ZSharp.Compiler.HIRReifier HIRReifier { get; set; } = new();
    }
}
