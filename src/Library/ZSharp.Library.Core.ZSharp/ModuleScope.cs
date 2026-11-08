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
        /// Exports a definition from the document it is defined in.
        /// </summary>
        [Alias("export")]
        public static global::ZSharp.HIR.Instance Export(global::ZSharp.HIR.Instance definition)
        {
            if (!Compiler.Contexts.Has<global::ZSharp.Lang.IExports>(out var exports))
                throw new InvalidOperationException("Only a definition in a document can be exported.");

            if (exports.Export(definition) is { } error)
                throw new InvalidOperationException(error.ToString());

            return definition;
        }
    }
}
