using CommonZ;
using ZSharp.Compiler;
using ZSharp.Importer.ILLoader;

using Code = ZSharp.HIR.Code;

namespace ZSharp.Lang.DotNet
{
    /// <summary>
    /// The CLR method meta-compiler. A first parameter named <c>this</c> makes it an
    /// instance method, and otherwise it is static.
    /// </summary>
    public sealed class MethodMetaType(
        Language language,
        ILLoader il,
        CodeBody body,
        Package.DotNet.TypeReference underlyingType
    )
    {
        public IResult<CompiledDefinition, Error> Compile(AST.Definition node, Code.Local self)
        {
            if (node is not AST.Function function)
                return Result<CompiledDefinition>.Error($"A {node.GetType().Name} is not a method.");

            return Result<CompiledDefinition>.Ok(new(
                self,
                CodeBody.Construct(underlyingType, new Code.StringLiteral(function.Name)),
                underlyingType,
                () => Fill(function, self)
            ));
        }

        private IResult<Code.Block, Error> Fill(AST.Function function, Code.Local self)
        {
            var signature = function.Signature;

            if (signature.VarArgs is not null || signature.KwArgs is not null || signature.VarKwArgs is not null)
                return Result<Code.Block>.Error($"Method {function.Name} has parameters that aren't supported yet.");

            var block = new Code.Block();
            var parameters = signature.Args ?? [];

            if (parameters is [{ Name: "this" }, ..])
            {
                var @this = new Code.Local("this", il.Load(typeof(Package.DotNet.This)));

                block.Locals.Add(@this);
                block.Statements.Add(CodeBody.Assign(@this, CodeBody.Construct(il.Load(typeof(Package.DotNet.This)))));
                block.Statements.Add(new Code.ExpressionStatement(new Code.Set
                {
                    Target = CodeBody.Member(self, nameof(Package.DotNet.Method.This)),
                    Value = new Code.Get(@this)
                }));

                language.Scope.Add("this", new Code.Get(@this));
            }
            else
                block.Statements.Add(new Code.ExpressionStatement(new Code.Set
                {
                    Target = CodeBody.Member(self, nameof(Package.DotNet.Method.IsStatic)),
                    Value = new Code.BooleanLiteral(true)
                }));

            if (body.Parameters(
                block,
                self,
                nameof(Package.DotNet.Method.Parameters),
                parameters.Where(parameter => parameter.Name != "this")
            ) is { } error)
                return Result<Code.Block>.Error(error);

            if (function.ReturnType is not null)
            {
                if (body.Described<HIR.Type>(function.ReturnType).When(out var type).Error(out error))
                    return Result<Code.Block>.Error(error);

                block.Statements.Add(new Code.ExpressionStatement(new Code.Set
                {
                    Target = CodeBody.Member(self, nameof(Package.DotNet.Method.ReturnType)),
                    Value = type!
                }));
            }

            return body.Body(block, self, function.Body, function.ReturnType is not null) is { } invalid
                ? Result<Code.Block>.Error(invalid)
                : Result<Code.Block>.Ok(block);
        }
    }
}
