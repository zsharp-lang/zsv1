namespace ZSharp.Lang
{
    /// <summary>
    /// A context that is a document.
    /// </summary>
    public interface IDocument
    {
        public Package.ZSharp.Document Document { get; }
    }

    /// <summary>
    /// A context that can export definitions.
    /// </summary>
    public interface IExports
    {
        /// <returns>Why the definition can't be exported, or <see langword="null"/>.</returns>
        public Error? Export(HIR.Instance definition);
    }

    /// <summary>
    /// The meta-compiler of a document. A document runs statement by statement: each
    /// statement is compiled and run before the next one is compiled. Nothing builds the
    /// code of a document, so the definitions in a statement are built, loaded and run
    /// right before it. As it goes, it fills in the <see cref="Package.ZSharp.Document"/>
    /// that importing the document gives. An import runs as its statement runs, so the
    /// statements after it know what it imported.
    /// </summary>
    public sealed class DocumentCompiler(Language language, string path = "")
    {
        /// <summary>
        /// The names of the document.
        /// </summary>
        public Scope Scope { get; } = new();

        public Package.ZSharp.Document Document { get; } = new(path);

        /// <summary>
        /// Compiles and runs one statement of the document, inside its context.
        /// </summary>
        public Error? Run(ASTCompiler compiler, HIRRunner runner, AST.Statement statement)
        {
            var context = new DocumentContext(this);

            compiler = WithImports(compiler);

            return compiler.Contexts.Within(context, () =>
            {
                if (Compile(compiler, statement, context).When(out var compiled).Error(out var error))
                    return error;

                try
                {
                    if (runner.RunStatement(compiled!) is { } failure)
                        return failure;
                }
                catch (InvalidOperationException exception)
                {
                    return new ErrorMessage(exception.Message);
                }

                return context.Publish();
            });
        }

        /// <summary>
        /// Compiles one statement of the document without running it.
        /// </summary>
        public IResult<HIRStatement, Error> Compile(ASTCompiler compiler, AST.Statement statement)
        {
            var context = new DocumentContext(this);

            compiler = WithImports(compiler);

            return compiler.Contexts.Within(context, () => Compile(compiler, statement, context));
        }

        /// <summary>
        /// A fork of the compiler in which an import is a statement of the document.
        /// </summary>
        private ASTCompiler WithImports(ASTCompiler compiler)
        {
            var fork = compiler.Fork();
            var previous = fork.Statement;

            fork.Statement = (current, statement) => statement is AST.ImportStatement import
                ? Import(current, import)
                : previous(current, statement);

            return fork;
        }

        private IResult<HIRStatement, Error> Import(ASTCompiler compiler, AST.ImportStatement import)
        {
            if (compiler.CompileExpression(import.Source).When(out var source).Error(out var error))
                return Result<HIRStatement>.Error(error);

            var result = new HIR.Code.Reference();

            if (language.BindImported(compiler, import, () => new HIR.Code.Get(result)) is { } invalid)
                return Result<HIRStatement>.Error(invalid);

            return Result<HIRStatement>.Ok(new Import(source!, result));
        }

        /// <summary>
        /// Compiles something as if it were in the document.
        /// </summary>
        public T Within<T>(ASTCompiler compiler, Func<T> compile)
            => compiler.Contexts.Within(new DocumentContext(this), compile);

        private static IResult<HIRStatement, Error> Compile(ASTCompiler compiler, AST.Statement statement, DocumentContext context)
        {
            var result = compiler.CompileStatement(statement);

            if (context.Defined.Count == 0 || result.When(out var compiled).Error(out _))
                return result;

            var block = new HIR.Code.Block();

            block.Statements.AddRange(context.Defined);
            block.Statements.Add(compiled!);

            return Result<HIRStatement>.Ok(block);
        }

        /// <summary>
        /// Loads a document as an object whose members are what it exports.
        /// </summary>
        public static void Install(MappedLoader loader, HIRLoader hir)
            => loader.Instances.Register<Package.ZSharp.Document>(
                document => Result<MIR.CompilerObject>.Ok(new DocumentObject(document, hir))
            );

        private sealed class DocumentContext(DocumentCompiler owner)
            : Context
            , INames
            , IBuildsDefinitions
            , IDocument
            , IExports
        {
            private readonly List<(string? Name, HIR.Code.Reference Result)> defines = [];
            private readonly List<HIR.Instance> exported = [];

            public Scope Names => owner.Scope;

            public Package.ZSharp.Document Document => owner.Document;

            public List<HIRStatement> Defined { get; } = [];

            public IResult<HIRExpression, Error> Build(CompiledDefinition definition, List<HIRExpression> decorators)
            {
                if (definition.Build().When(out var build).Error(out var error))
                    return Result<HIRExpression>.Error(error);

                var result = new HIR.Code.Reference();

                Defined.Add(new Define(Language.Decorate(build!, decorators), result));
                defines.Add((definition.Name, result));

                return Result<HIRExpression>.Ok(new HIR.Code.Get(result));
            }

            /// <summary>
            /// A definition is exported under the name it was defined with, which is known
            /// once the statement that defines it has run.
            /// </summary>
            public Error? Export(HIR.Instance definition)
            {
                exported.Add(definition);
                return null;
            }

            public Error? Publish()
            {
                foreach (var definition in exported)
                    if (defines.FirstOrDefault(define => ReferenceEquals(define.Result.Value, definition)) is { Name: { Length: > 0 } name })
                        owner.Document.Exports[name] = definition;
                    else
                        return new ErrorMessage("Only a named definition that this statement defines can be exported.");

                return null;
            }
        }

        private sealed class DocumentObject(Package.ZSharp.Document document, HIRLoader loader)
            : MIR.CompilerObject
            , ICTGetMember<string>
        {
            public IResult<MIR.CompilerObject, Error> Member(string member)
                => document.Exports.TryGetValue(member, out var exported)
                    ? loader.LoadInstance(exported)
                    : Result<MIR.CompilerObject>.Error(new UnsupportedError($"Document {document.Path} exports nothing named {member}."));
        }
    }
}
