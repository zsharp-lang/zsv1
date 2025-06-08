namespace ZSharp.Compiler
{
    public partial struct CG
    {
        public CompilerObjectResult ImplicitCast(CompilerObject @object, IType type)
        {
            var result = CompilerObjectResult.Error("Object does not support implicit cast");

            if (@object is ICTImplicitCastTo ctCastTo)
                result = ctCastTo.ImplicitCast(compiler, type);

            if (result.IsOk) return result;

            if (RuntimeDescriptor(@object, out var runtimeDescriptor)
                && runtimeDescriptor is IRTImplicitCastTo rtCastTo
            )
                result = rtCastTo.ImplicitCast(compiler, @object, type);

            if (result.IsOk) return result;

            if (type is ICTImplicitCastFrom ctCastFrom)
                result = ctCastFrom.ImplicitCast(compiler, @object);

            return result;
        }
    }
}
