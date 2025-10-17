using ZSharp.Compiler;
using ZSharp.IR;

namespace ZSharp.Importer.ILLoader.Objects
{
    internal sealed class StringType(TypeReference type, ILLoader loader)
        : CompilerObject
        , ICompileIRType
        , IRTImplicitCastTo
    {
        private readonly TypeReference type = type;
        private readonly ILLoader loader = loader;

        Result<IType> ICompileIRType.CompileIRType(Compiler.Compiler compiler)
            => Result<IType>.Ok(type);

        Result<CompilerObject> IRTImplicitCastTo.ImplicitCast(Compiler.Compiler compiler, CompilerObject @object, CompilerObject type)
        {
            if (compiler.Reflection.IsSameDefinition(this, type))
                return Result<CompilerObject>.Ok(@object);
           
            if (compiler.Reflection.IsSameDefinition(loader.TypeSystem.Object, type))
                return Result<CompilerObject>.Ok(@object);

            return Result<CompilerObject>.Error($"Cannot implicitly cast string to {type}.");
        }
    }
}
