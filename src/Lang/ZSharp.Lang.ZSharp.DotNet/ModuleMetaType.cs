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
                CodeBody.Construct(underlyingType, new Code.StringLiteral(module.Name)),
                underlyingType,
                () => container.Compile(
                    module.Body,
                    member => new Code.ExpressionStatement(CodeBody.Add(
                        self,
                        ReferenceEquals(member.UnderlyingType, il.Load(typeof(Package.DotNet.Method)))
                            ? nameof(Package.DotNet.Module.Methods)
                            : nameof(Package.DotNet.Module.Definitions),
                        new Code.Get(member.Variable)
                    ))
                )
            ));
        }
    }
}
