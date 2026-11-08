using CommonZ;
using ZSharp.Compiler;
using ZSharp.Importer.ILLoader;

using Code = ZSharp.HIR.Code;

namespace ZSharp.Lang.DotNet
{
    /// <summary>
    /// The CLR module meta-compiler. A module is one piece of code that produces a
    /// <see cref="Package.DotNet.Module"/>, so its definitions can refer to each other.
    /// </summary>
    public sealed class ModuleMetaType(
        ILLoader il,
        CodeBody body,
        ContainerCompiler container,
        Package.DotNet.TypeReference underlyingType
    )
    {
        public IResult<CompiledDefinition, Error> Compile(ASTCompiler compiler, AST.Definition node, Code.Local self)
        {
            if (node is not AST.Module module)
                return Result<CompiledDefinition>.Error($"A {node.GetType().Name} is not a module.");

            // In a module, an import runs as the module is constructed.
            var fork = compiler.Fork();
            var statements = fork.Statement;

            fork.Statement = (current, statement) => statement is AST.ImportStatement import
                ? body.Imported(current, import).When(out var holder).Error(out var error)
                    ? Result<Code.Statement>.Error(error)
                    : Result<Code.Statement>.Ok(new Code.Declare(holder!))
                : statements(current, statement);

            return Result<CompiledDefinition>.Ok(new(
                self,
                CodeBody.Construct(underlyingType, new Code.StringLiteral(module.Name)),
                underlyingType,
                () => container.Compile(
                    fork,
                    module.Body,
                    (member, value) => new Code.ExpressionStatement(CodeBody.Add(
                        self,
                        ReferenceEquals(member.UnderlyingType, il.Load(typeof(Package.DotNet.Method)))
                            ? nameof(Package.DotNet.Module.Methods)
                            : nameof(Package.DotNet.Module.Definitions),
                        value
                    ))
                )
            ));
        }
    }
}
