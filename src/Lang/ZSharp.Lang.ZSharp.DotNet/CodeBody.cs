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

        public IResult<Code.Expression, Error> Described<T>(ASTCompiler compiler, AST.Expression type)
            => Bodies.Described<T>(compiler, type);

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
        /// Adds code that builds each parameter of a CLR member, as the parameters of
        /// <c>HIR.Code</c>.
        /// </summary>
        public Error? Parameters(ASTCompiler compiler, Code.Block block, Code.Local owner, string list, AST.Signature signature)
            => Bodies.Parameters(compiler, block, owner, list, signature, il.Load(typeof(Code.PositionalParameter)), il.Load(typeof(Code.NamedParameter)));

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

        public static Code.NamedMember Member(Code.Local owner, string name)
            => Lang.Bodies.Member(owner, name);

        public static Code.ExpressionStatement Assign(Code.Local local, Code.Expression value)
            => Lang.Bodies.Assign(local, value);
    }
}
