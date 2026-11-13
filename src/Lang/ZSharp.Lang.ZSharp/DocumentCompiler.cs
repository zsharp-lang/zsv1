namespace ZSharp.Lang
{
    /// <summary>
    /// The meta-compiler of a document. A document runs statement by statement, and so
    /// does every statement nested in it: each one is compiled and run before the next
    /// one is compiled. Nothing builds the code of a document, so the definitions in a
    /// statement are built, loaded and run right before it, and an import runs as its
    /// statement runs. As it goes, it fills in the <see cref="Package.ZSharp.Document"/>
    /// that importing the document gives.
    /// <para>
    /// A <c>let</c> is a local of the document, which lives as long as the document. The
    /// host gives each document a frame that holds the values of its locals, and runs each
    /// statement with it.
    /// </para>
    /// </summary>
    public sealed partial class DocumentCompiler(Language language, IDocumentHost host, string path = "")
    {
        /// <summary>
        /// The names of the document.
        /// </summary>
        public Scope Scope { get; } = new();

        public Package.ZSharp.Document Document { get; } = new(path);

        /// <summary>
        /// Compiles and runs one statement of the document, inside its context.
        /// </summary>
        public Error? Run(ASTCompiler compiler, AST.Statement statement)
        {
            try
            {
                return Run(Forked(compiler), statement, Scope);
            }
            catch (InvalidOperationException exception)
            {
                return new ErrorMessage(exception.Message);
            }
        }

        /// <summary>
        /// Compiles one statement of the document without running it.
        /// </summary>
        public IResult<HIRStatement, Error> Compile(ASTCompiler compiler, AST.Statement statement)
        {
            var context = new DocumentContext(this, Scope);

            compiler = Forked(compiler);

            return compiler.Contexts.Within(context, () =>
            {
                var result = compiler.CompileStatement(statement);

                if (context.Defined.Count == 0 || result.When(out var compiled).Error(out _))
                    return result;

                var block = new HIR.Code.Block();

                block.Statements.AddRange(context.Defined);
                block.Statements.Add(compiled!);

                return Result<HIRStatement>.Ok(block);
            });
        }

        /// <summary>
        /// Compiles something as if it were in the document.
        /// </summary>
        public T Within<T>(ASTCompiler compiler, Func<T> compile)
            => compiler.Contexts.Within(new DocumentContext(this, Scope), compile);

        /// <summary>
        /// A name bound to a local of a document can be bound to another one, which is how
        /// a let reuses a name.
        /// </summary>
        public static void Install(MappedBinder binding)
        {
            binding.Merges.Add((existing, value) =>
                existing is HIR.Code.Get { Instance: Package.ZSharp.DocumentLocal }
                && value is HIR.Code.Get { Instance: Package.ZSharp.DocumentLocal }
                    ? value
                    : null
            );

            // A name bound again to what it already stands for, as importing it again does,
            // isn't a conflict.
            binding.Merges.Add((existing, value) => Same(existing, value) ? existing : null);
        }

        private static bool Same(HIRExpression a, HIRExpression b)
            => (a, b) switch
            {
                (HIR.Code.Get first, HIR.Code.Get second) => ReferenceEquals(first.Instance, second.Instance),
                (HIR.Code.NamedMember first, HIR.Code.NamedMember second) => first.Name == second.Name && Same(first.Target, second.Target),
                _ => false
            };

        private Error? Run(ASTCompiler compiler, AST.Statement statement, Scope names)
        {
            switch (statement)
            {
                case AST.BlockStatement block:
                    var inner = Remembered(block, () => new Scope(names));

                    foreach (var item in block.Statements)
                        if (Run(compiler, item, inner) is { } error)
                            return error;

                    return null;

                case AST.IfStatement @if:
                    if (Condition(compiler, @if.Condition, names, out var condition) is { } failure)
                        return failure;

                    if (condition)
                        return Run(compiler, @if.If, names);

                    return @if.Else is null ? null : Run(compiler, @if.Else, names);

                case AST.ExpressionStatement { Expression: AST.WhileExpression<AST.Statement> loop }:
                    return Loop(compiler, loop, names);

                case AST.BreakStatement leave when running.Count > 0:
                    if (leave.Value is not null)
                        return new ErrorMessage("Only a loop that is an expression can be left with a value.");

                    return Running(leave.From) is { } broken ? new Leaving(broken, next: false) : NoLoop("break", leave.From);

                case AST.ContinueStatement next when running.Count > 0:
                    return Running(next.From) is { } continued ? new Leaving(continued, next: true) : NoLoop("continue", next.From);

                default:
                    if (Remembered(statement) is HIRStatement known)
                        return Run(known);

                    var context = new DocumentContext(this, names);

                    return compiler.Contexts.Within(context, () =>
                    {
                        if (compiler.CompileStatement(statement).When(out var compiled).Error(out var error))
                            return error;

                        foreach (var defined in context.Defined)
                            if (Run(defined) is { } failed)
                                return failed;

                        Remember(statement, compiled!);

                        return Run(compiled!);
                    });
            }
        }

        /// <summary>
        /// Runs a statement of the document. Definitions and imports are its own, so it
        /// runs them, and the host runs the rest.
        /// </summary>
        private Error? Run(HIRStatement statement)
        {
            switch (statement)
            {
                case Define define:
                    if (host.Evaluate(Document, define.Build).When(out var value).Error(out var error))
                        return error;

                    // Code, as ct gives for a definition it loaded, has nothing to register.
                    if (value is HIR.Code.Expression)
                        return null;

                    if (value is Export export)
                    {
                        if ((export.Alias ?? define.Name) is not { Length: > 0 } name)
                            return new ErrorMessage("A definition with no name can only be exported with an alias.");

                        Document.Exports[name] = export.Definition;
                        value = export.Definition;
                    }

                    if (value is not HIR.Instance instance)
                        return new ErrorMessage($"A definition must produce an instance, but got {value?.GetType().Name ?? "null"}.");

                    define.Result.Value = instance;

                    // The document holds what it defines, so it understands what the
                    // definition says it is of the language, as an operator.
                    language.Understand(instance);

                    return host.Load(instance);

                case Import import:
                    if (host.Evaluate(Document, import.Importing).When(out var imported).Error(out error))
                        return error;

                    if (imported is not HIR.Instance from)
                        return new ErrorMessage($"An import must give an instance, but gave {imported?.GetType().Name ?? "null"}.");

                    import.Result.Value = from;

                    return null;

                default:
                    return host.Execute(Document, statement);
            }
        }

        /// <summary>
        /// A fork of the compiler in which an import is a statement of the document, and a
        /// let is a local of it.
        /// </summary>
        private ASTCompiler Forked(ASTCompiler compiler)
        {
            var fork = compiler.Fork();
            var statements = fork.Statement;
            var expressions = fork.Expression;

            fork.Statement = (current, statement) => statement is AST.ImportStatement import
                ? Import(current, import)
                : statements(current, statement);

            fork.Expression = (current, expression) => expression is AST.LetExpression let
                ? Local(current, let)
                : expressions(current, expression);

            return fork;
        }

        private IResult<HIRStatement, Error> Import(ASTCompiler compiler, AST.ImportStatement import)
        {
            if (language.Importing(compiler, import).When(out var importing).Error(out var error))
                return Result<HIRStatement>.Error(error);

            // The document is running, so an import that can run now does, and its names are
            // bound to what it gave, as anywhere else an import can run while compiling.
            if (language.CompileTime.Is(importing!))
            {
                if (host.Evaluate(Document, importing!).When(out var value).Error(out error))
                    return Result<HIRStatement>.Error(error);

                if (value is not HIR.Instance imported)
                    return Result<HIRStatement>.Error($"An import must give an instance, but gave {value?.GetType().Name ?? "null"}.");

                return language.BindImported(compiler, import, imported) is { } conflict
                    ? Result<HIRStatement>.Error(conflict)
                    : Result<HIRStatement>.Ok(new HIR.Code.Block());
            }

            var result = new HIR.Code.Reference();

            if (language.BindImported(compiler, import, () => new HIR.Code.Get(result)) is { } invalid)
                return Result<HIRStatement>.Error(invalid);

            return Result<HIRStatement>.Ok(new Import(importing!, result));
        }

        /// <summary>
        /// A local of the document. Its type is known before the statement runs, so a type
        /// written for it is evaluated now.
        /// </summary>
        private IResult<HIRExpression, Error> Local(ASTCompiler compiler, AST.LetExpression let)
        {
            HIR.Type type = HIR.Infer.Value;

            if (let.Type is not null)
            {
                if (compiler.CompileExpression(let.Type).When(out var compiled).Error(out var error))
                    return Result<HIRExpression>.Error(error);

                if (host.Evaluate(Document, host.Described(compiled!)).When(out var value).Error(out error))
                    return Result<HIRExpression>.Error(error);

                if (value is not HIR.Type described)
                    return Result<HIRExpression>.Error($"The type of local {let.Name} isn't a type.");

                type = described;
            }
            else if (let.Value is null)
                return Result<HIRExpression>.Error($"Local {let.Name} needs a type or a value.");

            HIRExpression? init = null;

            if (let.Value is not null && compiler.CompileExpression(let.Value).When(out init).Error(out var failure))
                return Result<HIRExpression>.Error(failure);

            var local = new Package.ZSharp.DocumentLocal(Document, let.Name, type) { Init = init };

            Document.Locals.Add(local);

            if (language.Bind(compiler, let.Name, new HIR.Code.Get(local)) is { } invalid)
                return Result<HIRExpression>.Error(invalid);

            return Result<HIRExpression>.Ok(new HIR.Code.BlockExpression
            {
                Statements = [new HIR.Code.Declare(local)],
                Result = new HIR.Code.Get(local)
            });
        }

        private sealed class DocumentContext(DocumentCompiler owner, Scope names)
            : Context
            , INames
            , IBuildsDefinitions
            , IDocument
        {
            public Scope Names => names;

            public Package.ZSharp.Document Document => owner.Document;

            public List<HIRStatement> Defined { get; } = [];

            public IResult<HIRExpression, Error> Build(CompiledDefinition definition, List<HIRExpression> decorators)
            {
                if (definition.Build().When(out var build).Error(out var error))
                    return Result<HIRExpression>.Error(error);

                var result = new HIR.Code.Reference { Kind = definition.UnderlyingType };

                Defined.Add(new Define(Language.Decorate(build!, decorators), result, definition.Name));

                return Result<HIRExpression>.Ok(new HIR.Code.Get(result));
            }
        }
    }
}
