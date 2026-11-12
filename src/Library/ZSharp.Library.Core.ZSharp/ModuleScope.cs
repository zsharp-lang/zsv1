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
        /// The <c>core:</c> library, whose modules are bound to the toolchain that is
        /// running. Z# adds its own to it, as a module of live objects is just their
        /// descriptions.
        /// </summary>
        [Alias("core")]
        public static global::ZSharp.Importer.String.LibraryImporter Libraries { get; set; } = null!;

        /// <summary>
        /// The <c>pkg:</c> library: the toolchain's own packages, which have no state, as
        /// <c>pkg:zsharp/hir</c>. What it has no module for is its fallback's, which the
        /// package manager sets.
        /// </summary>
        [Alias("packages")]
        public static global::ZSharp.Importer.String.LibraryImporter Packages { get; set; } = null!;

        /// <summary>
        /// What imports a source with no prefix, a file, by what is registered for its
        /// extension.
        /// </summary>
        [Alias("files")]
        public static global::ZSharp.Lang.FileImporter Files { get; set; } = null!;

        /// <summary>
        /// What routes a source to the importer of its prefix, which Z# can add to, as
        /// the package manager adds <c>pkg:</c>.
        /// </summary>
        [Alias("importers")]
        public static global::ZSharp.Importer.String.StringImporter Importers { get; set; } = null!;

        /// <summary>
        /// What runs documents, which can run one again, as a project's module file is.
        /// </summary>
        /// <summary>
        /// What describe an assembly as it is imported, each asked in turn, as one that
        /// reads what an encoder wrote of it: the first that knows it gives what importing
        /// it gives, and an assembly none knows is its module as the IL loader describes it.
        /// </summary>
        [Alias("enter")]
        public static Action<System.Reflection.Assembly> Enter { get; set; } = _ => { };

        [Alias("describers")]
        public static List<Func<System.Reflection.Assembly, global::ZSharp.HIR.Instance?>> Describers { get; } = [];

        [Alias("documents")]
        public static global::ZSharp.Lang.DocumentImporter Documents { get; set; } = null!;

        /// <summary>
        /// What the script the host runs was given: what follows it on the command line.
        /// </summary>
        [Alias("arguments")]
        public static IReadOnlyList<string> Arguments { get; set; } = [];

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
            => global::ZSharp.Compiler.Current.HIRReifier.EncodeValue(CT(new global::ZSharp.HIR.Code.Get(definition))).When(out var code).Error(out var error)
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
        /// Imports from an instance, which is what importing it gives, as a module a project
        /// compiled: <c>import { Cecil } from platform</c>.
        /// </summary>
        [Alias("import")]
        [Operator("import")]
        public static global::ZSharp.HIR.Instance Import(global::ZSharp.HIR.Instance instance)
            => instance;

        /// <summary>
        /// Marks a definition as an operation of the language, as in
        /// <c>@operator(Operator.Index)</c>.
        /// </summary>
        [Alias("operator")]
        public static Func<global::ZSharp.HIR.Instance, global::ZSharp.HIR.Instance> Operator(global::ZSharp.Lang.Operator @operator)
            => Marked(@operator);

        /// <summary>
        /// Marks a definition as one of the functions a piece of syntax stands for, as in
        /// <c>@operator((/))</c>, which is given as what an operator literal is.
        /// </summary>
        [Alias("operator")]
        public static Func<global::ZSharp.HIR.Instance, global::ZSharp.HIR.Instance> Operator(global::ZSharp.HIR.Instance @operator)
            => Marked(@operator);

        /// <summary>
        /// A decorator takes and gives the definition, so decorators compose. It says of the
        /// definition that it is the operator, as an annotation, which only a definition that
        /// is annotated can carry.
        /// </summary>
        private static Func<global::ZSharp.HIR.Instance, global::ZSharp.HIR.Instance> Marked(object @operator)
            => definition =>
            {
                if (definition is not global::ZSharp.HIR.Annotated annotated)
                    throw new InvalidOperationException("Only a definition that has annotations can be an operator.");

                annotated.Annotations.Add(new global::ZSharp.Lang.OperatorAnnotation(@operator));

                return definition;
            };

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
