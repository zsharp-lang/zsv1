using CommonZ;
using ZSharp.Compiler;

using Code = ZSharp.HIR.Code;

namespace ZSharp.Lang
{
    /// <summary>
    /// What meta-compilers of definitions with code share, whatever the definitions are:
    /// code that builds parameters and locals, and a body. Code in a body doesn't run while
    /// the body is being built, so it is reified: the code doing the building constructs it
    /// as HIR. Building code runs on the host, so the host's types are given by
    /// <paramref name="describe"/>, a value by <paramref name="expose"/>, and what builds
    /// nodes by <paramref name="nodes"/>.
    /// </summary>
    /// <param name="evaluate">Runs code while compiling.</param>
    public sealed class Bodies(
        Language language,
        Func<Type, HIR.Instance> describe,
        Func<object, HIR.Instance> expose,
        HIR.Instance nodes,
        Func<Code.Expression, IResult<object?, Error>> evaluate
    )
    {
        /// <summary>
        /// Compiles a type and casts it to what describes it, as in <c>I32 as Type</c>
        /// where <c>Type</c> is the HIR marker. An existing type is reflected, and a
        /// definition being built already is its description.
        /// </summary>
        public IResult<Code.Expression, Error> Described<T>(ASTCompiler compiler, AST.Expression type)
            => compiler.CompileExpression(type).When(out var compiled).Error(out var error)
                ? Result<Code.Expression>.Error(error)
                : Result<Code.Expression>.Ok(new Code.Cast { Value = compiled!, Type = new Code.Get(describe(typeof(T))) });

        /// <summary>
        /// Adds code that builds each parameter as a node of the given kinds, and binds its
        /// name. In <c>(a, b, {c, d})</c>, <c>a</c> and <c>b</c> are positional only and
        /// <c>c</c> and <c>d</c> named only. A parameter written <c>external as internal</c>
        /// is named <c>external</c> outside, which is what a named argument names, and
        /// <c>internal</c> in the code. A kind is constructed from the parameter's name, and
        /// its type if it has one, and has a <c>DefaultValue</c>.
        /// </summary>
        public Error? Parameters(ASTCompiler compiler, Code.Block block, Code.Local owner, string list, AST.Signature signature, HIR.Instance positional, HIR.Instance named)
        {
            if (signature.VarArgs is not null || signature.VarKwArgs is not null)
                return new ErrorMessage("Parameters that take the rest of the arguments aren't supported yet.");

            foreach (var parameter in signature.Args ?? [])
                if (Parameter(compiler, block, owner, list, parameter, positional) is { } error)
                    return error;

            foreach (var parameter in signature.KwArgs ?? [])
                if (Parameter(compiler, block, owner, list, parameter, named) is { } error)
                    return error;

            return null;
        }

        private Error? Parameter(ASTCompiler compiler, Code.Block block, Code.Local owner, string list, AST.Parameter parameter, HIR.Instance kind)
        {
            Code.Expression[] arguments = [new Code.StringLiteral(parameter.Name)];

            // A parameter with no type is completed by a rule or is an error, which the code
            // it belongs to decides when it is loaded.
            if (parameter.Type is not null)
                if (Described<HIR.Type>(compiler, parameter.Type).When(out var type).Error(out var error))
                    return error;
                else
                    arguments = [.. arguments, type!];

            var local = new Code.Local(parameter.Alias ?? parameter.Name, (HIR.Type)kind) { Init = Construct(kind, arguments) };

            block.Statements.Add(new Code.Declare(local));

            // A default value is code, which the call runs where the argument is left out.
            if (parameter.Initializer is not null)
                if (Reified(compiler, block, parameter.Initializer).When(out var initializer).Error(out var error))
                    return error;
                else
                    block.Statements.Add(new Code.ExpressionStatement(new Code.Set
                    {
                        Target = new Code.NamedMember { Target = new Code.Get(local), Name = "DefaultValue" },
                        Value = initializer!
                    }));

            // A parameter's decorators get it as it is built, and what they give is the
            // parameter: it may be of another kind, as an injected one is.
            if (parameter.Decorators.Count > 0)
            {
                if (Language.CompileDecorators(compiler, parameter.Decorators).When(out var decorators).Error(out var error))
                    return error;

                local = new Code.Local(local.Name, (HIR.Type)describe(typeof(Code.Parameter)))
                {
                    Init = Language.Decorate(new Code.Get(local), decorators!)
                };

                block.Statements.Add(new Code.Declare(local));
            }

            block.Statements.Add(new Code.ExpressionStatement(Add(owner, list, new Code.Get(local))));

            return language.Bind(compiler, parameter.Alias ?? parameter.Name, new Code.Get(local));
        }

        /// <summary>
        /// Adds code that sets the body of what is being built, its <c>Body</c>.
        /// </summary>
        public Error? Body(ASTCompiler compiler, Code.Block block, Code.Local owner, AST.Statement? body)
        {
            if (body is null)
                return new ErrorMessage("A body is required here.");

            if (BuiltBy(compiler, block, () => compiler.CompileStatement(body)).When(out var statement).Error(out var error))
                return error;

            var code = statement as Code.Block ?? new Code.Block(statement!);

            if (Core.Compiler.ModuleScope.HIRReifier.ReifyStatement(code).When(out var reified).Error(out error))
                return error;

            block.Statements.Add(new Code.ExpressionStatement(new Code.Set
            {
                Target = Member(owner, "Body"),
                Value = Node("CodeOf", reified!)
            }));

            return null;
        }

        /// <summary>
        /// Compiles code that belongs in a body and reifies it. The given block is what
        /// builds it.
        /// </summary>
        public IResult<Code.Expression, Error> Reified(ASTCompiler compiler, Code.Block block, AST.Expression expression)
            => BuiltBy(compiler, block, () => compiler.CompileExpression(expression)).When(out var compiled).Error(out var error)
                ? Result<Code.Expression>.Error(error)
                : Core.Compiler.ModuleScope.HIRReifier.ReifyExpression(compiled!);

        /// <summary>
        /// A fork of the compiler for a body, in which a <c>let</c> is a local of what is
        /// being built: the given block builds the local and adds it to the owner's list,
        /// and the body declares it where the <c>let</c> is. An import runs in the given
        /// block, so the body gets what it imported.
        /// </summary>
        /// <param name="function">
        /// What a function defined directly in the body is, or <see langword="null"/> for
        /// whatever a function is there anyway.
        /// </param>
        public ASTCompiler WithLocals(ASTCompiler compiler, Code.Block block, Code.Local owner, string list, Func<ASTCompiler, AST.Definition, IResult<CompiledDefinition, Error>>? function)
        {
            var fork = compiler.Fork();
            var expressions = fork.Expression;
            var statements = fork.Statement;
            var definitions = fork.Definition;
            var definition = DefinitionOf(compiler.Contexts.Current);

            // A definition inside another one, such as a method of a class defined in the
            // body, isn't the body's.
            if (function is not null)
                fork.Definition = (current, node) => node is AST.Function && DefinitionOf(current.Contexts.Current) == definition
                    ? function(current, node)
                    : definitions(current, node);

            fork.Expression = (current, expression) => expression is AST.LetExpression let
                ? Local(current, block, owner, list, let)
                : expressions(current, expression);

            // The import runs in the code that builds the body, so nothing of it is left
            // in the body.
            fork.Statement = (current, statement) => statement switch
            {
                AST.ImportStatement import => Imported(current, import).When(out var holder).Error(out var error)
                    ? Result<Code.Statement>.Error(error)
                    : holder is null ? Result<Code.Statement>.Ok(new Code.Block()) : Declared(block, holder),

                // A decorated statement's decorators are called as the body is built, and the
                // body gets what they gave in its place.
                AST.DecoratedStatement decorated when Language.Undecorate(decorated, out var decorators) is not AST.DefinitionStatement
                    => Language.Decorated(current, Language.Undecorate(decorated, out _), decorators).When(out var call).Error(out var failure)
                        ? Result<Code.Statement>.Error(failure)
                        : Result<Code.Statement>.Ok(new Code.UnquoteStatement(Node("Place", call!))),

                _ => statements(current, statement)
            };

            return fork;
        }

        /// <summary>
        /// The context of the definition that code in a context belongs to.
        /// </summary>
        public static Context? DefinitionOf(Context? context)
        {
            while (context is ChildContext child and not DefinitionContext)
                context = child.Parent;

            return context;
        }

        private static IResult<Code.Statement, Error> Declared(Code.Block block, Code.Local holder)
        {
            block.Statements.Add(new Code.Declare(holder));

            return Result<Code.Statement>.Ok(new Code.Block());
        }

        /// <summary>
        /// Compiles an import. One whose call is compile-time, as with a constant source,
        /// runs now, and the names it brings in are bound to what it gave, so they are known
        /// while compiling. Any other runs as the code is constructed: it compiles to a local
        /// that holds what it imports, which the names are bound to, and declaring the local
        /// runs the import. Gives that local, or <see langword="null"/> if the import ran now.
        /// </summary>
        public IResult<Code.Local?, Error> Imported(ASTCompiler compiler, AST.ImportStatement import)
        {
            if (language.Importing(compiler, import).When(out var importing).Error(out var error))
                return Result<Code.Local?>.Error(error);

            if (CompileTime.Is(importing!))
            {
                if (evaluate(importing!).When(out var value).Error(out error))
                    return Result<Code.Local?>.Error(error);

                if (value is not HIR.Instance imported)
                    return Result<Code.Local?>.Error($"An import must give an instance, but gave {value?.GetType().Name ?? "null"}.");

                return language.BindImported(compiler, import, imported) is { } conflict
                    ? Result<Code.Local?>.Error(conflict)
                    : Result<Code.Local?>.Ok(null);
            }

            var holder = new Code.Local("import", HIR.Infer.Value) { Init = importing };

            return language.BindImported(compiler, import, () => new Code.Get(holder)) is { } invalid
                ? Result<Code.Local?>.Error(invalid)
                : Result<Code.Local?>.Ok(holder);
        }

        private IResult<Code.Expression, Error> Local(
            ASTCompiler compiler,
            Code.Block block,
            Code.Local owner,
            string list,
            AST.LetExpression let
        )
        {
            Code.Expression type = new Code.Get(expose(HIR.Infer.Value));

            if (let.Type is not null)
                if (Described<HIR.Type>(compiler, let.Type).When(out var described).Error(out var error))
                    return Result<Code.Expression>.Error(error);
                else
                    type = described!;
            else if (let.Value is null)
                return Result<Code.Expression>.Error($"Local {let.Name} needs a type or a value.");

            var local = new Code.Local(let.Name, (HIR.Type)describe(typeof(Code.Local)))
            {
                Init = Node("Local", new Code.StringLiteral(let.Name), type)
            };

            block.Statements.Add(new Code.Declare(local));

            if (let.Value is not null)
                if (Reified(compiler, block, let.Value).When(out var value).Error(out var error))
                    return Result<Code.Expression>.Error(error);
                else
                    block.Statements.Add(new Code.ExpressionStatement(new Code.Set
                    {
                        Target = new Code.NamedMember { Target = new Code.Get(local), Name = nameof(Code.Local.Init) },
                        Value = value!
                    }));

            block.Statements.Add(new Code.ExpressionStatement(Add(owner, list, new Code.Get(local))));

            if (language.Bind(compiler, let.Name, new Code.Get(local)) is { } invalid)
                return Result<Code.Expression>.Error(invalid);

            // The body declares the local where the let is. That Declare is built here, so
            // it is already in the form of the building code.
            return Result<Code.Expression>.Ok(new Code.BlockExpression
            {
                Statements = [new Code.UnquoteStatement(Node("Declare", new Code.Get(local)))],
                Result = new Code.Get(local)
            });
        }

        /// <summary>
        /// Compiles code that the given block builds, so a definition inside it is built
        /// there.
        /// </summary>
        private static T BuiltBy<T>(ASTCompiler compiler, Code.Block block, Func<T> compile)
            => compiler.Contexts.Within(new BuildContext(compiler.Contexts.Current, block), compile);

        /// <summary>
        /// Code that calls what builds nodes on the host, by name.
        /// </summary>
        public Code.Call Node(string name, params Code.Expression[] arguments)
            => Call(new Code.NamedMember { Target = new Code.Get(nodes), Name = name }, arguments);

        public static Code.Call Construct(HIR.Instance type, params Code.Expression[] arguments)
            => Call(new Code.Get(type), arguments);

        public static Code.Call Add(Code.Local owner, string list, Code.Expression value)
            => Call(new Code.NamedMember { Target = Member(owner, list), Name = "Add" }, value);

        public static Code.NamedMember Member(Code.Local owner, string name)
            => new() { Target = new Code.Get(owner), Name = name };

        public static Code.ExpressionStatement Assign(Code.Local local, Code.Expression value)
            => new(new Code.Set { Target = new Code.Get(local), Value = value });

        public static Code.Call Call(Code.Expression callee, params Code.Expression[] arguments)
            => new()
            {
                Callee = callee,
                Arguments = [.. arguments.Select(argument => new Code.Argument { Value = argument })]
            };
    }
}
