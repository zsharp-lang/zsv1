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
        /// The meta-types definitions are compiled by, which Z# registers its own with and
        /// makes the default of a kind of definition.
        /// </summary>
        /// <summary>
        /// The language Z# is compiled with: its names, functions and definitions.
        /// </summary>
        [Alias("language")]
        public static global::ZSharp.Lang.Language Language { get; set; } = null!;

        [Alias("metaTypes")]
        public static global::ZSharp.Lang.MetaTypes MetaTypes { get; set; } = null!;

        /// <summary>
        /// What meta-compilers of definitions with code share: code that builds their
        /// parameters, locals and bodies.
        /// </summary>
        [Alias("bodies")]
        public static global::ZSharp.Lang.Bodies Bodies { get; set; } = null!;

        /// <summary>
        /// What imports from a source, giving the imported value or why it can't.
        /// </summary>
        public static Func<string, IResult<global::ZSharp.HIR.Instance, CommonZ.Error>> Importer { get; set; } = null!;

        /// <summary>
        /// What runs HIR now, for <c>ct</c>.
        /// </summary>
        public static Func<global::ZSharp.HIR.Code.Statement, CommonZ.Error?> Runner { get; set; } = null!;

        /// <summary>
        /// What evaluates HIR now, for <c>ct</c>.
        /// </summary>
        public static Func<global::ZSharp.HIR.Code.Expression, IResult<object?, CommonZ.Error>> Evaluator { get; set; } = null!;

        /// <summary>
        /// Evaluates code given as HIR, and gives what it evaluates to. As a decorator, it
        /// is called while the code around it is constructed.
        /// </summary>
        [Alias("ct")]
        public static object? CT(global::ZSharp.HIR.Code.Expression expression)
            => Evaluator(expression).When(out var value).Error(out var error)
                ? throw new InvalidOperationException(error.ToString())
                : value;

        /// <summary>
        /// Runs code given as HIR. A statement gives nothing.
        /// </summary>
        [Alias("ct")]
        public static void CT(global::ZSharp.HIR.Code.Statement statement)
        {
            if (Runner(statement) is { } error)
                throw new InvalidOperationException(error.ToString());
        }

        /// <summary>
        /// Loads a definition into the runtime, and gives code that refers to what it
        /// loaded. Code isn't a definition, so a container doesn't register it: a definition
        /// decorated with <c>ct</c> is loaded where it is, and not registered.
        /// </summary>
        [Alias("ct")]
        public static global::ZSharp.HIR.Code.Expression CT(global::ZSharp.HIR.Instance definition)
            => Core.Compiler.ModuleScope.HIRReifier.EncodeValue(CT(new global::ZSharp.HIR.Code.Get(definition))).When(out var code).Error(out var error)
                ? throw new InvalidOperationException(error.ToString())
                : code!;

        /// <summary>
        /// Loads a definition into the runtime and gives its runtime object.
        /// </summary>
        [Alias("load")]
        public static object? Load(global::ZSharp.HIR.Instance definition)
            => CT(new global::ZSharp.HIR.Code.Get(definition));

        /// <summary>
        /// Imports from a source. An import statement is a call of this, with the source
        /// and then the statement's own arguments.
        /// </summary>
        [Alias("import")]
        [Operator("import")]
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
