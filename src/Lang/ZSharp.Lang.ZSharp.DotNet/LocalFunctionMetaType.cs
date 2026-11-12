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
    /// as the code around it would resolve it, and if that is a variable of that code, or a
    /// member of one, as a member of the class is of the object a method runs on, the
    /// function captures it. The code around refers to a variable through a variable of the
    /// code that builds it, which the capture sets to a <see cref="Code.Capture"/> of what
    /// it held, before any code that refers to it is built. So the code around and the
    /// function both refer to the capture, and nothing is bound again. A function nested in
    /// this one resolves a name as this one does, so it captures what this one captured.
    /// </para>
    /// </summary>
    public sealed class LocalFunctionMetaType(
        ILLoader il,
        CodeBody body,
        Package.DotNet.TypeReference underlyingType
    )
    {
        private readonly HIR.Instance variable = il.Load(typeof(HIR.Variable));
        private readonly HIR.Instance capture = il.Load(typeof(Code.Capture));

        public IResult<CompiledDefinition, Error> Compile(ASTCompiler compiler, AST.Definition node, Code.Local self)
        {
            if (node is not AST.Function function)
                return Result<CompiledDefinition>.Error($"A {node.GetType().Name} is not a function.");

            if (!compiler.Contexts.Has<IBuildingCode>(out var around))
                return Result<CompiledDefinition>.Error($"Function {function.Name} needs code around it.");

            if (Language.Names(compiler) is not { } boundary)
                return Result<CompiledDefinition>.Error($"Function {function.Name} has no names of its own.");

            // A local function is a variable of the code around it, which that code refers to
            // through the definition's variable, as it refers to a local, so a function beside
            // it that uses it captures it. Its node is built through a variable of its own.
            self.Type = (HIR.Type)variable;

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
            var node = new Code.Local(self.Name, underlyingType)
            {
                Init = new Code.Cast { Value = new Code.Get(self), Type = new Code.Get(underlyingType) }
            };

            block.Statements.Add(new Code.Declare(node));

            var captures = new Captures(this, node, block, around);
            var fork = compiler.Fork();
            var expressions = fork.Expression;

            fork.Expression = (current, expression) =>
                expression is AST.IdentifierExpression identifier && !Inside(current, identifier.Name, boundary)
                    ? captures.Capture(expressions(current, expression))
                    : expressions(current, expression);

            if (body.Parameters(fork, block, node, nameof(Package.DotNet.LocalFunction.Parameters), signature) is { } error)
                return Result<Code.Block>.Error(error);

            if (function.ReturnType is not null)
            {
                if (fork.CompileExpression(function.ReturnType).When(out var type).Error(out error))
                    return Result<Code.Block>.Error(error);

                block.Statements.Add(new Code.ExpressionStatement(new Code.Set
                {
                    Target = CodeBody.Member(node, nameof(Package.DotNet.LocalFunction.ReturnType)),
                    Value = type!
                }));
            }

            var locals = body.WithLocals(fork, block, node, nameof(Package.DotNet.LocalFunction.Locals));

            return body.Body(locals, block, node, function.Body) is { } invalid
                ? Result<Code.Block>.Error(invalid)
                : Result<Code.Block>.Ok(block);
        }

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
        private sealed class Captures(LocalFunctionMetaType owner, Code.Local self, Code.Block block, Code.Block around)
        {
            private readonly HashSet<Code.Local> captured = new(ReferenceEqualityComparer.Instance);

            /// <param name="resolved">What the code around the function resolves the name to.</param>
            public IResult<Code.Expression, Error> Capture(IResult<Code.Expression, Error> resolved)
            {
                if (resolved.When(out var value).Error(out _))
                    return resolved;

                var holder = value switch
                {
                    Code.Get { Instance: Code.Local variable } => variable,
                    Code.NamedMember { Target: Code.Get { Instance: Code.Local variable } } => variable,
                    _ => null
                };

                if (holder is not null && ReferenceEquals(holder.Type, owner.variable) && captured.Add(holder))
                {
                    // The code around refers to its capture from now on, which this function
                    // has among its captures once it is built.
                    around.Statements.Add(new Code.ExpressionStatement(new Code.Set
                    {
                        Target = new Code.Get(holder),
                        Value = CodeBody.Construct(owner.capture, new Code.Get(holder))
                    }));

                    block.Statements.Add(new Code.ExpressionStatement(CodeBody.Add(
                        self,
                        nameof(Package.DotNet.LocalFunction.Captures),
                        new Code.Cast { Value = new Code.Get(holder), Type = new Code.Get(owner.capture) }
                    )));
                }

                return resolved;
            }
        }
    }
}
