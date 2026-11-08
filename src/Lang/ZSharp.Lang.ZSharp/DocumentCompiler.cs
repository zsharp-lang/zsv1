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
    /// What running a document needs from what runs it.
    /// </summary>
    public interface IDocumentHost
    {
        /// <summary>
        /// Runs a statement of the document, with the document's locals.
        /// </summary>
        public Error? Execute(Package.ZSharp.Document document, HIRStatement statement);

        /// <summary>
        /// Evaluates an expression of the document, with the document's locals.
        /// </summary>
        public IResult<object?, Error> Evaluate(Package.ZSharp.Document document, HIRExpression expression);

        /// <summary>
        /// Code that gives the HIR type that code evaluating to a type describes.
        /// </summary>
        public HIRExpression Described(HIRExpression type);

        /// <summary>
        /// Loads a definition that was just built, returning why it could not be.
        /// </summary>
        public Error? Load(HIR.Instance definition);
    }

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
    public sealed class DocumentCompiler(Language language, IDocumentHost host, string path = "")
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
        /// Loads a document as an object whose members are what it exports.
        /// </summary>
        public static void Install(MappedLoader loader, HIRLoader hir)
            => loader.Instances.Register<Package.ZSharp.Document>(
                document => Result<MIR.CompilerObject>.Ok(new DocumentObject(document, hir))
            );

        /// <summary>
        /// A name bound to a local of a document can be bound to another one, which is how
        /// a let reuses a name.
        /// </summary>
        public static void Install(MappedBinder binding)
            => binding.Merges.Add((existing, value) =>
                existing is HIR.Code.Get { Instance: Package.ZSharp.DocumentLocal }
                && value is HIR.Code.Get { Instance: Package.ZSharp.DocumentLocal }
                    ? value
                    : null
            );

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
        /// Runs a loop statement. Its body runs while its condition holds, and its else part
        /// runs when the condition stops holding, but not when a break leaves the loop. A
        /// statement in the loop is compiled the first time it is reached and runs on each
        /// iteration after that, and a block in it keeps its names, so what the first
        /// iteration defined is still defined.
        /// </summary>
        private Error? Loop(ASTCompiler compiler, AST.WhileExpression<AST.Statement> loop, Scope names)
        {
            var outermost = running.Count == 0;
            var self = new RunningLoop(loop.Name);

            if (outermost)
                remembered = new(ReferenceEqualityComparer.Instance);

            running.Push(self);

            try
            {
                while (true)
                {
                    if (Condition(compiler, loop.Condition, names, out var condition) is { } failure)
                        return failure;

                    if (!condition)
                        break;

                    switch (Run(compiler, loop.Body, names))
                    {
                        case null:
                            continue;
                        case Leaving { Next: true } next when next.Loop == self:
                            continue;
                        case Leaving { Next: false } leave when leave.Loop == self:
                            return null;
                        case { } error:
                            return error;
                    }
                }
            }
            finally
            {
                running.Pop();

                if (outermost)
                    remembered = null;
            }

            return loop.Else is null ? null : Run(compiler, loop.Else, names);
        }

        private Error? Condition(ASTCompiler compiler, AST.Expression expression, Scope names, out bool condition)
        {
            condition = false;

            if (Evaluate(compiler, expression, names).When(out var value).Error(out var error))
                return error;

            if (value is not bool result)
                return new ErrorMessage($"A condition must be a boolean, but got {value?.GetType().Name ?? "null"}.");

            condition = result;
            return null;
        }

        private IResult<object?, Error> Evaluate(ASTCompiler compiler, AST.Expression expression, Scope names)
        {
            if (Remembered(expression) is HIRExpression known)
                return host.Evaluate(Document, known);

            var context = new DocumentContext(this, names);

            return compiler.Contexts.Within(context, () =>
            {
                if (compiler.CompileExpression(expression).When(out var compiled).Error(out var error))
                    return Result<object?>.Error(error);

                foreach (var defined in context.Defined)
                    if (Run(defined) is { } failed)
                        return Result<object?>.Error(failed);

                Remember(expression, compiled!);

                return host.Evaluate(Document, compiled!);
            });
        }

        /// <summary>
        /// The loops running, innermost first.
        /// </summary>
        private readonly Stack<RunningLoop> running = [];

        /// <summary>
        /// While a loop runs, what each of its statements was compiled to, and the names of
        /// each of its blocks.
        /// </summary>
        private Dictionary<AST.Node, object>? remembered;

        private object? Remembered(AST.Node node)
            => remembered is not null && remembered.TryGetValue(node, out var known) ? known : null;

        private T Remembered<T>(AST.Node node, Func<T> make)
            where T : class
        {
            if (Remembered(node) is T known)
                return known;

            var made = make();
            Remember(node, made);
            return made;
        }

        private void Remember(AST.Node node, object value)
        {
            if (remembered is not null)
                remembered[node] = value;
        }

        private RunningLoop? Running(string? name)
            => name is null ? running.Peek() : running.FirstOrDefault(loop => loop.Name == name);

        private static ErrorMessage NoLoop(string statement, string? name)
            => new(name is null
                ? $"A {statement} needs a loop around it."
                : $"There is no loop named {name} around this {statement}.");

        private sealed class RunningLoop(string name)
        {
            public string Name { get; } = name;
        }

        /// <summary>
        /// A break or continue on its way to the loop it names. It is not an error, but it
        /// leaves every statement between them the way one does.
        /// </summary>
        private sealed class Leaving(RunningLoop loop, bool next)
            : ErrorMessage(next ? $"A continue of loop {loop.Name} got outside it." : $"A break of loop {loop.Name} got outside it.")
        {
            public RunningLoop Loop { get; } = loop;

            public bool Next { get; } = next;
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

                var result = new HIR.Code.Reference();

                Defined.Add(new Define(Language.Decorate(build!, decorators), result, definition.Name));

                return Result<HIRExpression>.Ok(new HIR.Code.Get(result));
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
