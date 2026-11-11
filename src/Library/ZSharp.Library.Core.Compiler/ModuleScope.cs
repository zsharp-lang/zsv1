using ZSharp.Compiler;
using ZSharp.Importer.ILLoader;

namespace Core.Compiler
{
    /// <summary>
    /// <c>core:compiler</c>: the compiler in use, <see cref="Current"/>, as Z# reaches it.
    /// </summary>
    [ModuleScope]
    public static class ModuleScope
    {
        [Alias("cg")]
        public static ZSharp.Compiler.CG CG { get => Current.CG; set => Current.CG = value; }

        [Alias("ts")]
        public static ZSharp.Compiler.TS TS { get => Current.TS; set => Current.TS = value; }

        [Alias("overloading")]
        public static ZSharp.Compiler.Overloading Overloading { get => Current.Overloading; set => Current.Overloading = value; }

        [Alias("ir")]
        public static ZSharp.Compiler.IR IR { get => Current.IR; set => Current.IR = value; }

        [Alias("hirLoader")]
        public static ZSharp.Compiler.HIRLoader HIRLoader { get => Current.HIRLoader; set => Current.HIRLoader = value; }

        [Alias("hirCodeLoader")]
        public static ZSharp.Compiler.HIRCodeLoader HIRCodeLoader { get => Current.HIRCodeLoader; set => Current.HIRCodeLoader = value; }

        [Alias("mappedLoader")]
        public static ZSharp.Compiler.MappedLoader MappedLoader { get => Current.MappedLoader; set => Current.MappedLoader = value; }

        [Alias("bindings")]
        public static ZSharp.Compiler.Bindings Bindings { get => Current.Bindings; set => Current.Bindings = value; }

        [Alias("mappedCodeLoader")]
        public static ZSharp.Compiler.MappedCodeLoader MappedCodeLoader { get => Current.MappedCodeLoader; set => Current.MappedCodeLoader = value; }

        [Alias("hirReifier")]
        public static ZSharp.Compiler.HIRReifier HIRReifier { get => Current.HIRReifier; set => Current.HIRReifier = value; }
    }
}
