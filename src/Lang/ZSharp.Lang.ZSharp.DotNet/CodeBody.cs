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
    /// <param name="importer">Code that gives what imports from a source.</param>
    public sealed class CodeBody(
        Language language,
        ILLoader il,
        Func<Code.Expression> importer
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
        /// Adds code that builds each parameter, and binds its name.
        /// </summary>
        public Error? Parameters(ASTCompiler compiler, Code.Block block, Code.Local owner, string list, IEnumerable<AST.Parameter> parameters)
        {
            foreach (var parameter in parameters)
            {
                Code.Expression[] arguments = [new Code.StringLiteral(parameter.Name)];

                // A parameter with no type is completed by a rule or is an error, which
                // the code it belongs to decides when it is loaded.
                if (parameter.Type is not null)
                    if (Described<HIR.Type>(compiler, parameter.Type).When(out var type).Error(out var error))
                        return error;
                    else
                        arguments = [.. arguments, type!];

                var local = new Code.Local(parameter.Name, il.Load(typeof(Package.DotNet.Parameter)))
                {
                    Init = Construct(il.Load(typeof(Package.DotNet.Parameter)), arguments)
                };

                block.Statements.Add(new Code.Declare(local));
                block.Statements.Add(new Code.ExpressionStatement(Add(owner, list, new Code.Get(local))));

                if (language.Bind(compiler, parameter.Name, new Code.Get(local)) is { } invalid)
                    return invalid;
            }

            return null;
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

            fork.Expression = (current, expression) => expression is AST.LetExpression let
                ? Local(current, block, owner, list, let)
                : expressions(current, expression);


            // The import runs in the code that builds the body, so nothing of it is left
            // in the body.
            fork.Statement = (current, statement) => statement is AST.ImportStatement import
                ? Imported(current, import).When(out var holder).Error(out var error)
                    ? Result<Code.Statement>.Error(error)
                    : Declared(block, holder!)
                : statements(current, statement);

            return fork;
        }

        private static IResult<Code.Statement, Error> Declared(Code.Block block, Code.Local holder)
        {
            block.Statements.Add(new Code.Declare(holder));

            return Result<Code.Statement>.Ok(new Code.Block());
        }

        /// <summary>
        /// Compiles an import to a local that holds what it imports, and binds the names
        /// it brings in to that local. Declaring the local runs the import.
        /// </summary>
        public IResult<Code.Local, Error> Imported(ASTCompiler compiler, AST.ImportStatement import)
        {
            if (compiler.CompileExpression(import.Source).When(out var source).Error(out var error))
                return Result<Code.Local>.Error(error);

            var holder = new Code.Local("import", HIR.Infer.Value) { Init = Call(importer(), source!) };

            return language.BindImported(compiler, import, () => new Code.Get(holder)) is { } invalid
                ? Result<Code.Local>.Error(invalid)
                : Result<Code.Local>.Ok(holder);
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
