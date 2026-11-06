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
        ContainerCompiler container,
        Package.DotNet.TypeReference underlyingType
    )
    {
        public IResult<CompiledDefinition, Error> Compile(AST.Definition node, Code.Local self)
        {
            if (node is not AST.Module module)
                return Result<CompiledDefinition>.Error($"A {node.GetType().Name} is not a module.");

            return Result<CompiledDefinition>.Ok(new(
                self,
                new Code.Call
                {
                    Callee = new Code.Get(underlyingType),
                    Arguments = [new() { Value = new Code.StringLiteral(module.Name) }]
                },
                underlyingType,
                () => container.Compile(
                    module.Body,
                    definition => new Code.ExpressionStatement(new Code.Call
                    {
                        Callee = new Code.NamedMember
                        {
                            Target = new Code.NamedMember
                            {
                                Target = new Code.Get(self),
                                Name = nameof(Package.DotNet.Module.Definitions)
                            },
                            Name = "Add"
                        },
                        Arguments = [new() { Value = definition }]
                    })
                )
            ));
        }
    }
}
