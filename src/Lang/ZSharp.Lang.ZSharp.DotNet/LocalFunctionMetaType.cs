using CommonZ;
using ZSharp.Compiler;
using ZSharp.Importer.ILLoader;

using Code = ZSharp.HIR.Code;

namespace ZSharp.Lang.DotNet
{
    /// <summary>
    /// The meta-compiler of a function defined in the code of a method. It captures the
    /// variables of the code around it that it uses, by reference, as C# does.
    /// <para>
    /// A name bound inside the function is the function's own. Any other name is resolved
    /// as the code around it would resolve it, and if that is a variable of that code, the
    /// function captures it: the code around it gets a <see cref="Code.Capture"/> of the
    /// variable, bound over the variable's name, so it and the function both refer to the
    /// capture from then on, and the variable is marked as captured. A capture found that
    /// way is shared, so a variable is captured once, however many functions use it.
    /// </para>
    /// </summary>
    public sealed class LocalFunctionMetaType(
        Language language,
        ILLoader il,
        CodeBody body,
        Package.DotNet.TypeReference underlyingType
    )
    {
        private readonly HIR.Instance local = il.Load(typeof(Code.Local));
        private readonly HIR.Instance capture = il.Load(typeof(Code.Capture));
        private readonly HIR.Instance positional = il.Load(typeof(Code.PositionalParameter));
        private readonly HIR.Instance named = il.Load(typeof(Code.NamedParameter));

        public IResult<CompiledDefinition, Error> Compile(ASTCompiler compiler, AST.Definition node, Code.Local self)
        {
            if (node is not AST.Function function)
                return Result<CompiledDefinition>.Error($"A {node.GetType().Name} is not a function.");

            if (!compiler.Contexts.Has<IBuildingCode>(out var around))
                return Result<CompiledDefinition>.Error($"Function {function.Name} needs code around it.");

            if (Language.Names(compiler) is not { } boundary)
                return Result<CompiledDefinition>.Error($"Function {function.Name} has no names of its own.");

            return Result<CompiledDefinition>.Ok(new(
                self,
                CodeBody.Construct(underlyingType, new Code.StringLiteral(function.Name ?? string.Empty)),
                underlyingType,
                () => Fill(compiler, function, self, around.Block, boundary)
            ));
        }

        private IResult<Code.Block, Error> Fill(
            ASTCompiler compiler,
            AST.Function function,
            Code.Local self,
            Code.Block around,
            Scope boundary
        )
        {
            var signature = function.Signature;

            var block = new Code.Block();
            var captures = new Captures(this, self, block, around, boundary);
            var fork = compiler.Fork();
            var expressions = fork.Expression;

            fork.Expression = (current, expression) =>
                expression is AST.IdentifierExpression identifier && !Inside(current, identifier.Name, boundary)
                    ? captures.Capture(identifier.Name, expressions(current, expression))
                    : expressions(current, expression);

            if (body.Parameters(fork, block, self, nameof(Package.DotNet.LocalFunction.Parameters), signature) is { } error)
                return Result<Code.Block>.Error(error);

            if (function.ReturnType is not null)
            {
                if (body.Described<HIR.Type>(fork, function.ReturnType).When(out var type).Error(out error))
                    return Result<Code.Block>.Error(error);

                block.Statements.Add(new Code.ExpressionStatement(new Code.Set
                {
                    Target = CodeBody.Member(self, nameof(Package.DotNet.LocalFunction.ReturnType)),
                    Value = type!
                }));
            }

            var locals = body.WithLocals(fork, block, self, nameof(Package.DotNet.LocalFunction.Locals));

            return body.Body(locals, block, self, function.Body) is { } invalid
                ? Result<Code.Block>.Error(invalid)
                : Result<Code.Block>.Ok(block);
        }

        private Error? Bind(Scope scope, string name, Code.Expression value)
            => language.Bind(scope, name, value);

        /// <summary>
        /// Whether a name is bound inside the function, which is every scope up to and
        /// including the function's own.
        /// </summary>
        private static bool Inside(ASTCompiler compiler, string name, Scope boundary)
        {
            for (var scope = Language.Names(compiler); scope is not null; scope = scope.Parent)
            {
                if (scope.Local(name) is not null)
                    return true;

                if (scope == boundary)
                    return false;
            }

            return false;
        }

        /// <summary>
        /// The captures of one function as they are found.
        /// </summary>
        private sealed class Captures(
            LocalFunctionMetaType owner,
            Code.Local self,
            Code.Block block,
            Code.Block around,
            Scope boundary
        )
        {
            private readonly HashSet<Code.Local> captured = new(ReferenceEqualityComparer.Instance);

            /// <param name="resolved">What the code around the function resolves the name to.</param>
            public IResult<Code.Expression, Error> Capture(string name, IResult<Code.Expression, Error> resolved)
            {
                if (resolved.When(out var value).Error(out _) || value is not Code.Get { Instance: Code.Local holder })
                    return resolved;

                if (ReferenceEquals(holder.Type, owner.capture))
                    return Share(holder);

                if (ReferenceEquals(holder.Type, owner.positional) || ReferenceEquals(holder.Type, owner.named))
                    return Result<Code.Expression>.Error($"Capturing parameter {name} isn't supported yet.");

                if (!ReferenceEquals(holder.Type, owner.local))
                    return resolved;

                if (Binding(name, holder) is not { } scope)
                    return Result<Code.Expression>.Error($"The scope {name} is bound in can't be found.");

                var made = new Code.Local(name, (HIR.Type)owner.capture)
                {
                    Init = CodeBody.Construct(owner.capture, new Code.Get(holder))
                };

                around.Statements.Add(new Code.Declare(made));
                around.Statements.Add(new Code.ExpressionStatement(new Code.Set
                {
                    Target = new Code.NamedMember { Target = new Code.Get(holder), Name = nameof(Code.Local.IsCaptured) },
                    Value = new Code.BooleanLiteral(true)
                }));

                if (owner.Bind(scope, name, new Code.Get(made)) is { } invalid)
                    return Result<Code.Expression>.Error(invalid);

                return Share(made);
            }

            private IResult<Code.Expression, Error> Share(Code.Local holder)
            {
                if (captured.Add(holder))
                    block.Statements.Add(new Code.ExpressionStatement(
                        CodeBody.Add(self, nameof(Package.DotNet.LocalFunction.Captures), new Code.Get(holder))
                    ));

                return Result<Code.Expression>.Ok(new Code.Get(holder));
            }

            /// <summary>
            /// The scope outside the function that binds a name to a variable.
            /// </summary>
            private Scope? Binding(string name, Code.Local holder)
            {
                for (var scope = boundary.Parent; scope is not null; scope = scope.Parent)
                    if (scope.Local(name) is Code.Get { Instance: var bound } && ReferenceEquals(bound, holder))
                        return scope;

                return null;
            }
        }
    }
}
