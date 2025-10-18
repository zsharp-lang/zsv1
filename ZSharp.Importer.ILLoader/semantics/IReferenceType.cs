using ZSharp.Compiler;

namespace ZSharp.Importer.ILLoader
{
    internal interface IReferenceType
        : IILType
        , IRTImplicitCastTo
    {
        Result<CompilerObject> IRTImplicitCastTo.ImplicitCast(Compiler.Compiler compiler, CompilerObject @object, CompilerObject type)
        {
            var thisIL = GetILType();

            if (!type.Is<IILType>(out var ilTypeProvider))
                return Result.Error(
                    $"Cannot implicitly cast type {thisIL.Name} to non-IL type {type}"
                );

            var thatIL = ilTypeProvider.GetILType();

            if (!thisIL.IsAssignableTo(thatIL))
                return Result.Error(
                    $"Cannot implicitly cast type {thisIL.Name} to incompatible type {thatIL.Name}"
                );

            return Result.Ok(@object);
        }
    }
}
