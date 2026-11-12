using CommonZ;
using ZSharp.Compiler;
using ZSharp.Importer.ILLoader;

using Code = ZSharp.HIR.Code;

namespace ZSharp.Lang.DotNet
{
    /// <summary>
    /// What the meta-compilers of CLR members with code share: the CLR's rules for fields,
    /// type parameters, members of a container and functions in a body, over what any
    /// code shares, <see cref="Lang.Bodies"/>.
    /// </summary>
    /// <param name="function">Compiles a function defined directly in a body.</param>
    /// <param name="method">Compiles a function defined directly in a CLR class or module.</param>
    /// <param name="evaluate">Runs code while compiling.</param>
    public sealed class CodeBody(
        Language language,
        ILLoader il,
        Func<ASTCompiler, AST.Definition, IResult<CompiledDefinition, Error>> function,
        Func<ASTCompiler, AST.Definition, IResult<CompiledDefinition, Error>> method,
        Func<Code.Expression, IResult<object?, Error>> evaluate
    )
    {
        /// <summary>
        /// What any code shares, built on the CLR host: its types are described by the IL
        /// loader, and its nodes built by <see cref="Nodes"/>.
        /// </summary>
        public Bodies Bodies { get; } = new(language, type => il.Load(type), value => il.Expose(value), il.Load(typeof(Nodes)), evaluate);

        /// <summary>
        /// A compiler for the members of a CLR class, interface or module: a <c>fun</c> in it
        /// is a CLR method, whatever <c>fun</c> is elsewhere. A definition inside a member
        /// is the member's to compile.
        /// </summary>
        public ASTCompiler WithMethods(ASTCompiler compiler)
        {
            var fork = compiler.Fork();
            var definitions = fork.Definition;
            var owner = Bodies.DefinitionOf(compiler.Contexts.Current);

            fork.Definition = (current, node) => node is AST.Function && Bodies.DefinitionOf(current.Contexts.Current) == owner
                ? method(current, node)
                : definitions(current, node);

            return fork;
        }

        /// <summary>
        /// Code that builds the type parameters of a generic definition and adds them to the
        /// owner's list, binding each name, so what is compiled after them can use them.
        /// </summary>
        public IResult<List<Code.Statement>, Error> TypeParameters(ASTCompiler compiler, List<AST.GenericParameter>? parameters, Code.Local owner, string list)
        {
            var statements = new List<Code.Statement>();
            var kind = il.Load(typeof(Package.DotNet.GenericParameter));

            foreach (var parameter in parameters ?? [])
            {
                if (parameter.Constraint is not null)
                    return Result<List<Code.Statement>>.Error($"Type parameter {parameter.Name} has a constraint, which isn't supported yet.");

                var holder = new Code.Local(parameter.Name, kind)
                {
                    Init = Construct(kind, new Code.StringLiteral(parameter.Name))
                };

                statements.Add(new Code.Declare(holder));
                statements.Add(new Code.ExpressionStatement(Add(owner, list, new Code.Get(holder))));

                if (language.Bind(compiler, parameter.Name, new Code.Get(holder)) is { } error)
                    return Result<List<Code.Statement>>.Error(error);
            }

            return Result<List<Code.Statement>>.Ok(statements);
        }

        /// <summary>
        /// Adds code that builds each parameter of a CLR member, as the CLR has them: an
        /// argument with its name fills it, or else the next positional one, or else its
        /// default value. A CLR parameter can always be given by position, so one written
        /// to be named only has no meaning on the CLR.
        /// </summary>
        public Error? Parameters(ASTCompiler compiler, Code.Block block, Code.Local owner, string list, AST.Signature signature)
        {
            if (signature.KwArgs is { Count: > 0 } named)
                return new ErrorMessage($"Parameter {named[0].Name} is named only, which a parameter of the CLR's can't be: each can be given by position or by name.");

            var parameter = il.Load(typeof(Package.DotNet.Parameter));
            return Bodies.Parameters(compiler, block, owner, list, signature, parameter, parameter);
        }

        /// <summary>
        /// Binds the names of the members of the class a method or a constructor is defined
        /// in, for its code, to the members of its <c>this</c>, the object it runs on, which
        /// what lowers the code gives: a member of the object, or the class's if it has none,
        /// as C# resolves a simple name. They are bound once its signature is compiled, as
        /// they are its code's names and not its signature's, and what the code names itself,
        /// as a parameter, hides one.
        /// </summary>
        public Error? Members(ASTCompiler compiler, Code.Block block, Code.Local owner, AST.Signature signature)
        {
            var names = ClassMembers.Of(compiler);

            if (names.Count == 0)
                return null;

            var receiver = new Code.Local("this", (HIR.Type)il.Load(typeof(Package.DotNet.This)))
            {
                Init = Member(owner, nameof(Package.DotNet.Method.This))
            };

            block.Statements.Add(new Code.Declare(receiver));

            HashSet<string> hidden = [.. (signature.Args ?? []).Concat(signature.KwArgs ?? []).Select(parameter => parameter.Alias ?? parameter.Name)];

            foreach (var name in names)
                if (!hidden.Contains(name) && Language.Names(compiler)?.Local(name) is null
                    && language.Bind(compiler, name, new Code.NamedMember { Target = new Code.Get(receiver), Name = name }) is { } error)
                    return error;

            return null;
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

            if (compiler.CompileExpression(let.Type).When(out var type).Error(out var error))
                return Result<Code.Expression>.Error(error);

            var fieldType = il.Load(typeof(Package.DotNet.Field));
            var field = new Code.Local(let.Name, fieldType)
            {
                Init = Construct(fieldType, new Code.StringLiteral(let.Name), type!)
            };
            var block = new Code.Block();

            block.Statements.Add(new Code.Declare(field));
            block.Statements.Add(Accessible(field, MemberDefaults.FieldsIn(compiler)));

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
        /// A container's fields: a <c>let</c> in it is a field, which decorators on it are
        /// given, as a definition is, rather than the code that builds it.
        /// </summary>
        public ASTCompiler WithFields(ASTCompiler compiler, Code.Local owner, string list, bool bind)
        {
            var fork = compiler.Fork();
            var expressions = fork.Expression;
            var statements = fork.Statement;

            fork.Expression = (current, expression) => expression is AST.LetExpression let
                ? Field(current, let, owner, list, bind)
                : expressions(current, expression);

            fork.Statement = (current, statement) =>
            {
                if (Language.Undecorate(statement, out var decorators) is not AST.ExpressionStatement { Expression: AST.LetExpression let } || decorators.Count == 0)
                    return statements(current, statement);

                if (Field(current, let, owner, list, bind).When(out var field).Error(out var error))
                    return Result<Code.Statement>.Error(error);

                return Language.CompileDecorators(current, decorators).When(out var compiled).Error(out error)
                    ? Result<Code.Statement>.Error(error)
                    : Result<Code.Statement>.Ok(new Code.ExpressionStatement(Language.Decorate(field!, compiled!)));
            };

            return fork;
        }

        public Error? Body(ASTCompiler compiler, Code.Block block, Code.Local owner, AST.Statement? body)
            => Bodies.Body(compiler, block, owner, body);

        public IResult<Code.Expression, Error> Reified(ASTCompiler compiler, Code.Block block, AST.Expression expression)
            => Bodies.Reified(compiler, block, expression);

        /// <summary>
        /// A fork of the compiler for the body of a CLR member: a function defined directly
        /// in it is a local function of the body, which captures by reference, as in C#.
        /// </summary>
        public ASTCompiler WithLocals(ASTCompiler compiler, Code.Block block, Code.Local owner, string list)
            => Bodies.WithLocals(compiler, block, owner, list, function);

        public IResult<Code.Local?, Error> Imported(ASTCompiler compiler, AST.ImportStatement import)
            => Bodies.Imported(compiler, import);

        public Code.Call Node(string name, params Code.Expression[] arguments)
            => Bodies.Node(name, arguments);

        public static Code.Call Construct(HIR.Instance type, params Code.Expression[] arguments)
            => Lang.Bodies.Construct(type, arguments);

        public static Code.Call Add(Code.Local owner, string list, Code.Expression value)
            => Lang.Bodies.Add(owner, list, value);

        /// <summary>
        /// Code that registers what a definition's decorators gave in one of the owner's lists.
        /// </summary>
        public Code.Call Register(Code.Local owner, string list, Code.Expression value)
            => Lang.Bodies.Call(new Code.NamedMember { Target = new Code.Get(il.Load(typeof(Registration))), Name = nameof(Registration.Register) }, Member(owner, list), value);

        /// <summary>
        /// Code that gives a definition being built its accessibility, which its decorators
        /// can change after it.
        /// </summary>
        public Code.Statement Accessible(Code.Local owner, Package.DotNet.Accessibility accessibility)
            => new Code.ExpressionStatement(new Code.Set
            {
                Target = Member(owner, nameof(Package.DotNet.Accessible.Accessibility)),
                Value = new Code.Get(il.Expose(accessibility))
            });

        public static Code.NamedMember Member(Code.Local owner, string name)
            => Lang.Bodies.Member(owner, name);

        public static Code.ExpressionStatement Assign(Code.Local local, Code.Expression value)
            => Lang.Bodies.Assign(local, value);
    }
}
