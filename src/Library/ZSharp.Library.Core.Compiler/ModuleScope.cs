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

        [Alias("overloading")]
        public static ZSharp.Compiler.Overloading Overloading { get; set; } = new();

        [Alias("ir")]
        public static ZSharp.Compiler.IR IR { get; set; } = new();

        [Alias("hirLoader")]
        public static ZSharp.Compiler.HIRLoader HIRLoader { get; set; } = new();

        [Alias("hirCodeLoader")]
        public static ZSharp.Compiler.HIRCodeLoader HIRCodeLoader { get; set; } = new();

        /// <summary>
        /// What loads HIR nodes by their type, which Z# registers the loaders of its own
        /// nodes with. The host sets it once it has made it.
        /// </summary>
        [Alias("mappedLoader")]
        public static ZSharp.Compiler.MappedLoader MappedLoader { get; set; } = null!;

        /// <summary>
        /// What binds each kind of node to what it was built as, which libraries register
        /// for their nodes, and an importer of built definitions uses.
        /// </summary>
        [Alias("bindings")]
        public static ZSharp.Compiler.Bindings Bindings { get; set; } = new();

        /// <summary>
        /// What loads HIR code by the type of its nodes.
        /// </summary>
        [Alias("mappedCodeLoader")]
        public static ZSharp.Compiler.MappedCodeLoader MappedCodeLoader { get; set; } = null!;

        [Alias("hirReifier")]
        public static ZSharp.Compiler.HIRReifier HIRReifier { get; set; } = new();
    }
}
