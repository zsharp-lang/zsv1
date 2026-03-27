using ZSharp.Compiler;
using ZSharp.Compiler.Features;
using ZSharp.IR;

namespace ZSharp.Importer.ILLoader.Objects
{
    public sealed class ObjectType(TypeReference<IR.Class> type)
        : CompilerObject
        , ICompileIRType<TypeReference<IR.Class>>
        , IILType
        , ISingleInheritance
    {
        private readonly TypeReference<IR.Class> type = type;

        CompilerObject? ISingleInheritance.Base => null;

        IResult<TypeReference<IR.Class>, Error> ICompileIRType<TypeReference<IR.Class>>.CompileIRType(Compiler.Compiler compiler, object? target)
            => Result<TypeReference<IR.Class>>.Ok(type);

        Type IILType.GetILType()
            => typeof(object);
    }
}
