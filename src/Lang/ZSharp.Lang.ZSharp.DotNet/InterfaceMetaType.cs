using CommonZ;
using ZSharp.Compiler;
using ZSharp.Importer.ILLoader;

using Code = ZSharp.HIR.Code;

namespace ZSharp.Lang.DotNet
{
    /// <summary>
    /// The CLR interface meta-compiler, for <c>class I of DotNet_Interface</c>. Its members
    /// are methods, which have no code of their own, and its bases are interfaces it extends.
    /// </summary>
    public sealed class InterfaceMetaType(
        ILLoader il,
        ContainerCompiler container,
        CodeBody body,
        ClassMetaType classes,
        Package.DotNet.TypeReference underlyingType
    )
    {
        public IResult<CompiledDefinition, Error> Compile(ASTCompiler compiler, AST.Definition node, Code.Local self)
        {
            if (node is not AST.TypeDefinition definition)
                return Result<CompiledDefinition>.Error($"A {node.GetType().Name} is not an interface.");

            return Result<CompiledDefinition>.Ok(new(
                self,
                CodeBody.Construct(underlyingType, new Code.StringLiteral(definition.Name)),
                underlyingType,
                () => Fill(compiler, definition, self)
            ));
        }

        private IResult<Code.Block, Error> Fill(ASTCompiler compiler, AST.TypeDefinition definition, Code.Local self)
        {
            // An interface is public, and so are its members unless they say otherwise.
            var filled = MemberDefaults.Within(compiler, Package.DotNet.Accessibility.Public, Package.DotNet.Accessibility.Public, () => container.Compile(
                body.WithMethods(compiler),
                definition.Content?.Statements ?? [],
                (member, value) => new Code.ExpressionStatement(body.Register(self, nameof(Package.DotNet.Interface.Methods), value))
            ));

            if (filled.When(out var block).Error(out var error))
                return Result<Code.Block>.Error(error);

            block!.Statements.Insert(0, body.Accessible(self, Package.DotNet.Accessibility.Public));

            return classes.Bases(compiler, definition, self, block!) is { } invalid ? Result<Code.Block>.Error(invalid) : filled;
        }
    }
}
