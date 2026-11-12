using CommonZ;
using ZSharp.Compiler;
using ZSharp.Importer.ILLoader;

using Code = ZSharp.HIR.Code;

namespace ZSharp.Lang.DotNet
{
    /// <summary>
    /// The CLR module meta-compiler. A module is one piece of code that produces a
    /// <see cref="Package.DotNet.Module"/>, so its definitions can refer to each other. A
    /// <c>fun</c> in it is a function of the module and a <c>let</c> a variable of it,
    /// which its functions reach by name.
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
            // A module is any definition with a body, as `module Name { ... }` or
            // `def Name of DotNet_Module { ... }`.
            var (name, content) = node switch
            {
                AST.Module module => (module.Name, module.Body),
                AST.TypeDefinition definition => (definition.Name, definition.Content?.Statements ?? []),
                _ => (null!, null!)
            };

            if (content is null)
                return Result<CompiledDefinition>.Error($"A {node.GetType().Name} is not a module.");

            // In a module, an import runs as the module is constructed, unless it can run now,
            // and a fun is a function of the module.
            var fork = body.WithFields(body.WithMethods(compiler), self, nameof(Package.DotNet.Module.Fields), bind: true);
            var statements = fork.Statement;

            fork.Statement = (current, statement) => statement is AST.ImportStatement import
                ? body.Imported(current, import).When(out var holder).Error(out var error)
                    ? Result<Code.Statement>.Error(error)
                    : Result<Code.Statement>.Ok(holder is null ? new Code.Block() : new Code.Declare(holder))
                : statements(current, statement);

            return Result<CompiledDefinition>.Ok(new(
                self,
                CodeBody.Construct(underlyingType, new Code.StringLiteral(name)),
                underlyingType,
                () => container.Compile(
                    fork,
                    content,
                    (member, value) => new Code.ExpressionStatement(body.Register(
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
