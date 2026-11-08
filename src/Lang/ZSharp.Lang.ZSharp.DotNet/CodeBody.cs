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
        ILLoader il
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

                var local = new Code.Local(parameter.Name, il.Load(typeof(Package.DotNet.Parameter)));

                block.Locals.Add(local);
                block.Statements.Add(Assign(local, Construct(il.Load(typeof(Package.DotNet.Parameter)), arguments)));
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
