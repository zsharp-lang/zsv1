using ZSharp.Importer.ILLoader;

namespace Core.ZSharp
{
    /// <summary>
    /// The Z# language as seen through <c>core:zsharp</c>. The CLI sets these once it has
    /// created them.
    /// </summary>
    [ModuleScope]
    public static class ModuleScope
    {
        [Alias("compiler")]
        public static global::ZSharp.Compiler.ASTCompiler Compiler { get; set; } = null!;

        /// <summary>
        /// What imports from a source, giving the imported value or why it can't.
        /// </summary>
        public static Func<string, IResult<global::ZSharp.HIR.Instance, CommonZ.Error>> Importer { get; set; } = null!;

        /// <summary>
        /// Imports from a source. This is what an import in code that is built does
        /// when that code is constructed.
        /// </summary>
        [Alias("import")]
        public static global::ZSharp.HIR.Instance Import(string source)
            => Importer(source).When(out var imported).Error(out var error)
                ? throw new InvalidOperationException(error.ToString())
                : imported!;

        /// <summary>
        /// Marks a definition to be exported under its name.
        /// </summary>
        [Alias("export")]
        public static global::ZSharp.Lang.Export Export(global::ZSharp.HIR.Instance definition)
            => new(definition, null);

        /// <summary>
        /// Marks a definition to be exported under an alias, as in <c>@export("name")</c>.
        /// </summary>
        [Alias("export")]
        public static Func<global::ZSharp.HIR.Instance, global::ZSharp.Lang.Export> Export(string alias)
            => definition => new(definition, alias);
    }
}
