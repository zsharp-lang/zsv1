using CommonZ;
using ZSharp.Compiler;
using ZSharp.Importer.ILLoader;

using Code = ZSharp.HIR.Code;

namespace ZSharp.Lang.DotNet
{
    /// <summary>
    /// The CLR constructor meta-compiler. A constructor always has <c>this</c>.
    /// </summary>
    public sealed class ConstructorMetaType(
        ILLoader il,
        CodeBody body,
        Package.DotNet.TypeReference underlyingType
    )
    {
        public IResult<CompiledDefinition, Error> Compile(ASTCompiler compiler, AST.Definition node, Code.Local self)
        {
            if (node is not AST.Constructor constructor)
                return Result<CompiledDefinition>.Error($"A {node.GetType().Name} is not a constructor.");

            return Result<CompiledDefinition>.Ok(new(
                self,
                CodeBody.Construct(underlyingType),
                underlyingType,
                () => Fill(compiler, constructor, self)
            ));
        }

        private IResult<Code.Block, Error> Fill(ASTCompiler compiler, AST.Constructor constructor, Code.Local self)
        {
            var signature = constructor.Signature;

            var block = new Code.Block();

            if (body.Parameters(
                compiler,
                block,
                self,
                nameof(Package.DotNet.Constructor.Parameters),
                signature
            ) is { } error)
                return Result<Code.Block>.Error(error);

            return body.Body(body.WithLocals(compiler, block, self, nameof(Package.DotNet.Constructor.Locals)), block, self, constructor.Body) is { } invalid
                ? Result<Code.Block>.Error(invalid)
                : Result<Code.Block>.Ok(block);
        }
    }
}
