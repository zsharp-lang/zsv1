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
        Language language,
        ILLoader il,
        CodeBody body,
        Package.DotNet.TypeReference underlyingType
    )
    {
        public IResult<CompiledDefinition, Error> Compile(AST.Definition node, Code.Local self)
        {
            if (node is not AST.Constructor constructor)
                return Result<CompiledDefinition>.Error($"A {node.GetType().Name} is not a constructor.");

            return Result<CompiledDefinition>.Ok(new(
                self,
                CodeBody.Construct(underlyingType),
                underlyingType,
                () => Fill(constructor, self)
            ));
        }

        private IResult<Code.Block, Error> Fill(AST.Constructor constructor, Code.Local self)
        {
            var signature = constructor.Signature;

            if (signature.VarArgs is not null || signature.KwArgs is not null || signature.VarKwArgs is not null)
                return Result<Code.Block>.Error("A constructor has parameters that aren't supported yet.");

            var block = new Code.Block();
            var @this = new Code.Local("this", il.Load(typeof(Package.DotNet.This)));

            block.Locals.Add(@this);
            block.Statements.Add(CodeBody.Assign(@this, CodeBody.Member(self, nameof(Package.DotNet.Constructor.This))));

            language.Scope.Add("this", new Code.Get(@this));

            if (body.Parameters(
                block,
                self,
                nameof(Package.DotNet.Constructor.Parameters),
                (signature.Args ?? []).Where(parameter => parameter.Name != "this")
            ) is { } error)
                return Result<Code.Block>.Error(error);

            return body.Body(block, self, constructor.Body, returnsValue: false) is { } invalid
                ? Result<Code.Block>.Error(invalid)
                : Result<Code.Block>.Ok(block);
        }
    }
}
