using CommonZ;
using ZSharp.Compiler;
using ZSharp.Importer.ILLoader;

using Code = ZSharp.HIR.Code;

namespace ZSharp.Lang.DotNet
{
    /// <summary>
    /// What the meta-compilers of members with code share. Code in a body doesn't run
    /// while the body is being built, so it is reified: the code doing the building
    /// constructs it as HIR.
    /// </summary>
    /// <param name="function">Compiles a function defined directly in a body.</param>
    /// <param name="evaluate">Runs code while compiling.</param>
    public sealed class CodeBody(
        Language language,
        ILLoader il,
        Func<ASTCompiler, AST.Definition, IResult<CompiledDefinition, Error>> function,
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
                : Result<Code.Expression>.Ok(new Code.Cast { Value = compiled!, Type = new Code.Get(il.Load(typeof(T))) });

        /// <summary>
        /// Adds code that builds each parameter, and binds its name. In <c>(a, b, {c, d})</c>,
        /// <c>a</c> and <c>b</c> are positional only and <c>c</c> and <c>d</c> named only. A
        /// parameter written <c>external as internal</c> is named <c>external</c> outside,
        /// which is what a named argument names, and <c>internal</c> in the code.
        /// </summary>
        public Error? Parameters(ASTCompiler compiler, Code.Block block, Code.Local owner, string list, AST.Signature signature)
        {
            if (signature.VarArgs is not null || signature.VarKwArgs is not null)
                return new ErrorMessage("Parameters that take the rest of the arguments aren't supported yet.");

            foreach (var parameter in signature.Args ?? [])
                if (Parameter(compiler, block, owner, list, parameter, il.Load(typeof(Code.PositionalParameter))) is { } error)
                    return error;

            foreach (var parameter in signature.KwArgs ?? [])
                if (Parameter(compiler, block, owner, list, parameter, il.Load(typeof(Code.NamedParameter))) is { } error)
                    return error;

            return null;
        }

        private Error? Parameter(ASTCompiler compiler, Code.Block block, Code.Local owner, string list, AST.Parameter parameter, Package.DotNet.TypeReference kind)
        {
            Code.Expression[] arguments = [new Code.StringLiteral(parameter.Name)];

            // A parameter with no type is completed by a rule or is an error, which the code
            // it belongs to decides when it is loaded.
            if (parameter.Type is not null)
                if (Described<HIR.Type>(compiler, parameter.Type).When(out var type).Error(out var error))
                    return error;
                else
                    arguments = [.. arguments, type!];

            var local = new Code.Local(parameter.Alias ?? parameter.Name, kind) { Init = Construct(kind, arguments) };

            block.Statements.Add(new Code.Declare(local));

            // A default value is code, which the call runs where the argument is left out.
            if (parameter.Initializer is not null)
                if (Reified(compiler, block, parameter.Initializer).When(out var initializer).Error(out var error))
                    return error;
                else
                    block.Statements.Add(new Code.ExpressionStatement(new Code.Set
                    {
                        Target = new Code.NamedMember { Target = new Code.Get(local), Name = nameof(Code.PositionalParameter.DefaultValue) },
                        Value = initializer!
                    }));

            // A parameter's decorators get it as it is built, and what they give is the
            // parameter: it may be of another kind, as an injected one is.
            if (parameter.Decorators.Count > 0)
            {
                if (Language.CompileDecorators(compiler, parameter.Decorators).When(out var decorators).Error(out var error))
                    return error;

                local = new Code.Local(local.Name, il.Load(typeof(Code.Parameter)))
                {
                    Init = Language.Decorate(new Code.Get(local), decorators!)
                };

                block.Statements.Add(new Code.Declare(local));
            }

            block.Statements.Add(new Code.ExpressionStatement(Add(owner, list, new Code.Get(local))));

            return language.Bind(compiler, parameter.Alias ?? parameter.Name, new Code.Get(local));
        }

        /// <summary>
        /// Compiles a <c>let</c> of a container to code that builds a field and adds it to
        /// the owner's list. Its initial value is code, reified for whoever runs it. A class
        /// reaches its fields through an object, so only a container whose fields its code
        /// reaches by name, such as a module, binds the name.
        /// </summary>
        public IResult<Code.Expression, Error> Field(ASTCompiler compiler, AST.LetExpression let, Code.Local owner, string list, bool bind)
        {
            if (let.Type is null)
                return Result<Code.Expression>.Error($"Field {let.Name} needs a type.");

            if (Described<HIR.Type>(compiler, let.Type).When(out var type).Error(out var error))
                return Result<Code.Expression>.Error(error);

            var fieldType = il.Load(typeof(Package.DotNet.Field));
            var field = new Code.Local(let.Name, fieldType)
            {
                Init = Construct(fieldType, new Code.StringLiteral(let.Name), type!)
            };
            var block = new Code.Block();

            block.Statements.Add(new Code.Declare(field));

            if (let.Value is not null)
            {
                if (Reified(compiler, block, let.Value).When(out var value).Error(out error))
                    return Result<Code.Expression>.Error(error);

                block.Statements.Add(new Code.ExpressionStatement(new Code.Set
                {
                    Target = Member(field, nameof(Package.DotNet.Field.DefaultValue)),
                    Value = value!
                }));
            }

            block.Statements.Add(new Code.ExpressionStatement(Add(owner, list, new Code.Get(field))));

            if (bind && language.Bind(compiler, let.Name, new Code.Get(field)) is { } invalid)
                return Result<Code.Expression>.Error(invalid);

            return Result<Code.Expression>.Ok(new Code.BlockExpression
            {
                Statements = block.Statements,
                Result = new Code.Get(field)
            });
        }

        /// <summary>
        /// Adds code that sets the body of what is being built.
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
        public ASTCompiler WithLocals(ASTCompiler compiler, Code.Block block, Code.Local owner, string list)
        {
            var fork = compiler.Fork();
            var expressions = fork.Expression;
            var statements = fork.Statement;
            var definitions = fork.Definition;
            var definition = Definition(compiler.Contexts.Current);

            // A function defined directly in the body is a function of the body, not a
            // method. A definition inside another one, such as a method of a class defined
            // in the body, isn't the body's.
            fork.Definition = (current, node) => node is AST.Function && Definition(current.Contexts.Current) == definition
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
                        : Result<Code.Statement>.Ok(new Code.UnquoteStatement(Node(nameof(Nodes.Place), call!))),

                _ => statements(current, statement)
            };

            return fork;
        }

        /// <summary>
        /// The context of the definition that code in a context belongs to.
        /// </summary>
        private static Context? Definition(Context? context)
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

                return language.BindImported(compiler, import, () => new Code.Get(imported)) is { } conflict
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
            Code.Expression type = new Code.Get(il.Expose(HIR.Infer.Value));

            if (let.Type is not null)
                if (Described<HIR.Type>(compiler, let.Type).When(out var described).Error(out var error))
                    return Result<Code.Expression>.Error(error);
                else
                    type = described!;
            else if (let.Value is null)
                return Result<Code.Expression>.Error($"Local {let.Name} needs a type or a value.");

            var local = new Code.Local(let.Name, il.Load(typeof(Code.Local)))
            {
                Init = Node(nameof(Nodes.Local), new Code.StringLiteral(let.Name), type)
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
                Statements = [new Code.UnquoteStatement(Node(nameof(Nodes.Declare), new Code.Get(local)))],
                Result = new Code.Get(local)
            });
        }

        /// <summary>
        /// Compiles code that the given block builds, so a definition inside it is built
        /// there.
        /// </summary>
        private static T BuiltBy<T>(ASTCompiler compiler, Code.Block block, Func<T> compile)
            => compiler.Contexts.Within(new BuildContext(compiler.Contexts.Current, block), compile);

        public Code.Call Node(string name, params Code.Expression[] arguments)
            => Call(new Code.NamedMember { Target = new Code.Get(il.Load(typeof(Nodes))), Name = name }, arguments);

        public static Code.Call Construct(HIR.Instance type, params Code.Expression[] arguments)
            => Call(new Code.Get(type), arguments);

        public static Code.Call Add(Code.Local owner, string list, Code.Expression value)
            => Call(new Code.NamedMember { Target = Member(owner, list), Name = "Add" }, value);

        /// <summary>
        /// Code that registers what a definition's decorators gave in one of the owner's lists.
        /// </summary>
        public Code.Call Register(Code.Local owner, string list, Code.Expression value)
            => Call(new Code.NamedMember { Target = new Code.Get(il.Load(typeof(Registration))), Name = nameof(Registration.Register) }, Member(owner, list), value);

        public static Code.NamedMember Member(Code.Local owner, string name)
            => new() { Target = new Code.Get(owner), Name = name };

        public static Code.ExpressionStatement Assign(Code.Local local, Code.Expression value)
            => new(new Code.Set { Target = new Code.Get(local), Value = value });

        private static Code.Call Call(Code.Expression callee, params Code.Expression[] arguments)
            => new()
            {
                Callee = callee,
                Arguments = [.. arguments.Select(argument => new Code.Argument { Value = argument })]
            };
    }
}
