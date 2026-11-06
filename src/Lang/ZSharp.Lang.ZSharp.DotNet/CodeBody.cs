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
    public sealed class CodeBody(
        Language language,
        ASTCompiler compiler,
        MappedASTCompiler mapped,
        ILLoader il
    )
    {
        /// <summary>
        /// Compiles a type and casts it to what describes it, as in <c>I32 as Type</c>
        /// where <c>Type</c> is the HIR marker. An existing type is reflected, and a
        /// definition being built already is its description.
        /// </summary>
        public IResult<Code.Expression, Error> Described<T>(AST.Expression type)
            => compiler.CompileExpression(type).When(out var compiled).Error(out var error)
                ? Result<Code.Expression>.Error(error)
                : Result<Code.Expression>.Ok(new Code.Cast { Value = compiled!, Type = new Code.Get(il.Load(typeof(T))) });

        /// <summary>
        /// Adds code that builds each parameter, and binds its name.
        /// </summary>
        public Error? Parameters(Code.Block block, Code.Local owner, string list, IEnumerable<AST.Parameter> parameters)
        {
            foreach (var parameter in parameters)
            {
                if (parameter.Type is null)
                    return new ErrorMessage($"Parameter {parameter.Name} needs a type.");

                if (Described<HIR.Type>(parameter.Type).When(out var type).Error(out var error))
                    return error;

                var local = new Code.Local(parameter.Name, il.Load(typeof(Package.DotNet.Parameter)));

                block.Locals.Add(local);
                block.Statements.Add(Assign(local, Construct(
                    il.Load(typeof(Package.DotNet.Parameter)),
                    new Code.StringLiteral(parameter.Name),
                    type!
                )));
                block.Statements.Add(new Code.ExpressionStatement(Add(owner, list, new Code.Get(local))));

                language.Scope.Add(parameter.Name, new Code.Get(local));
            }

            return null;
        }

        /// <summary>
        /// Adds code that sets the body of what is being built.
        /// </summary>
        public Error? Body(Code.Block block, Code.Local owner, AST.Statement? body, bool returnsValue)
        {
            if (body is null)
                return new ErrorMessage("A body is required here.");

            if (InBody(() => compiler.CompileStatement(body)).When(out var statement).Error(out var error))
                return error;

            var code = statement as Code.Block ?? new Code.Block(statement!);

            if (Returns.Check(code, returnsValue) is { } invalid)
                return invalid;

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
        /// Compiles code that belongs in a body and reifies it.
        /// </summary>
        public IResult<Code.Expression, Error> Reified(AST.Expression expression)
            => InBody(() => compiler.CompileExpression(expression)).When(out var compiled).Error(out var error)
                ? Result<Code.Expression>.Error(error)
                : Core.Compiler.ModuleScope.HIRReifier.ReifyExpression(compiled!);

        /// <summary>
        /// In a body, a definition inside an expression is back to being something the
        /// code around it doesn't contain.
        /// </summary>
        private T InBody<T>(Func<T> compile)
        {
            var typeDefinitions = mapped.Expressions.Register<AST.TypeDefinition>(language.CompileDefinition);
            var modules = mapped.Expressions.Register<AST.Module>(language.CompileDefinition);
            var functions = mapped.Expressions.Register<AST.Function>(language.CompileDefinition);

            try
            {
                return compile();
            }
            finally
            {
                mapped.Expressions.Register<AST.TypeDefinition>(typeDefinitions!);
                mapped.Expressions.Register<AST.Module>(modules!);
                mapped.Expressions.Register<AST.Function>(functions!);
            }
        }

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
