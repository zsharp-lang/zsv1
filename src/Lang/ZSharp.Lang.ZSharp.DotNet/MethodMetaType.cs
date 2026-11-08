using CommonZ;
using ZSharp.Compiler;
using ZSharp.Importer.ILLoader;

using Code = ZSharp.HIR.Code;

namespace ZSharp.Lang.DotNet
{
    /// <summary>
    /// The CLR method meta-compiler. A method is an instance method unless something
    /// marks it static, and no name is bound to the object it runs on.
    /// </summary>
    public sealed class MethodMetaType(
        ILLoader il,
        CodeBody body,
        Package.DotNet.TypeReference underlyingType
    )
    {
        public IResult<CompiledDefinition, Error> Compile(ASTCompiler compiler, AST.Definition node, Code.Local self)
        {
            if (node is not AST.Function function)
                return Result<CompiledDefinition>.Error($"A {node.GetType().Name} is not a method.");

            return Result<CompiledDefinition>.Ok(new(
                self,
                CodeBody.Construct(underlyingType, new Code.StringLiteral(function.Name)),
                underlyingType,
                () => Fill(compiler, function, self)
            ));
        }

        private IResult<Code.Block, Error> Fill(ASTCompiler compiler, AST.Function function, Code.Local self)
        {
            var signature = function.Signature;

            var block = new Code.Block();

            if (body.Parameters(
                compiler,
                block,
                self,
                nameof(Package.DotNet.Method.Parameters),
                signature
            ) is { } error)
                return Result<Code.Block>.Error(error);

            if (function.ReturnType is not null)
            {
                if (body.Described<HIR.Type>(compiler, function.ReturnType).When(out var type).Error(out error))
                    return Result<Code.Block>.Error(error);

                block.Statements.Add(new Code.ExpressionStatement(new Code.Set
                {
                    Target = CodeBody.Member(self, nameof(Package.DotNet.Method.ReturnType)),
                    Value = type!
                }));
            }

            // A method with no body has no code of its own: it is abstract, or an interface's.
            if (function.Body is null)
                return Result<Code.Block>.Ok(block);

            return body.Body(body.WithLocals(compiler, block, self, nameof(Package.DotNet.Method.Locals)), block, self, function.Body) is { } invalid
                ? Result<Code.Block>.Error(invalid)
                : Result<Code.Block>.Ok(block);
        }
    }
}
