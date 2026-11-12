namespace ZSharp.Compiler
{
    /// <summary>
    /// The compiler in use: its operations, the REIs, and its loaders, which the host makes
    /// once and what compiles reaches. It belongs to the compiler, which is defined in terms
    /// of its operations; <c>core:compiler</c> is how Z# reaches it, and not where it is.
    /// </summary>
    public static class Current
    {
        public static CG CG { get; set; } = new();

        public static TS TS { get; set; } = new();

        public static Overloading Overloading { get; set; } = new();

        public static IR IR { get; set; } = new();

        public static HIRLoader HIRLoader { get; set; } = new();

        public static HIRCodeLoader HIRCodeLoader { get; set; } = new();

        /// <summary>
        /// What loads HIR nodes by their type, which Z# registers the loaders of its own
        /// nodes with. The host sets it once it has made it.
        /// </summary>
        public static MappedLoader MappedLoader { get; set; } = null!;

        /// <summary>
        /// What binds each kind of node to what it was built as, which libraries register
        /// for their nodes, and an importer of built definitions uses.
        /// </summary>
        public static Bindings Bindings { get; set; } = new();

        /// <summary>
        /// What loads HIR code by the type of its nodes. The host sets it once it has made it.
        /// </summary>
        public static MappedCodeLoader MappedCodeLoader { get; set; } = null!;

        public static HIRReifier HIRReifier { get; set; } = new();
    }
}
