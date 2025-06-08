namespace ZSharp.Compiler
{
    public partial struct CG
    {
        public Result<TypeCast, Error> Cast(CompilerObject @object, IType type)
        {
            var result = Result<TypeCast, Error>.Error(
                "Object cannot be cast"
            );

            if (@object is ICTCastTo ctCastTo)
                result = ctCastTo.Cast(compiler, type);

            if (result.IsOk) return result;

            if (
                RuntimeDescriptor(@object, out var runtimeDescriptor)
                && runtimeDescriptor is IRTCastTo rtCastTo
            )
                result = rtCastTo.Cast(compiler, @object, type);

            if (result.IsOk) return result;

            if (type is IRTCastFrom rtCastFrom)
                result = rtCastFrom.Cast(compiler, @object);

            if (result.IsOk) return result;

            if (compiler.TypeSystem.IsTyped(@object, out var objectType) && 
                compiler.TypeSystem.AreEqual(type, objectType)
            )
                result = Result<TypeCast, Error>.Ok(new()
                {
                    Cast = @object,
                }); // TODO: this is actually invalid because calling code expects OnCast to be called on cast

            return result;
        }
    }
}
