namespace ZSharp.Compiler
{
    public partial struct IR
    {
        public Result<T, Error> CompileReference<T>(CompilerObject @object)
            where T : class
        {
            T? result = null;

            if (@object is ICompileIRReference<T> irReference)
                result = irReference.CompileIRReference(compiler);

            if (result is not null)
                return Result<T, Error>.Ok(result);
            return Result<T, Error>.Error(
                "Object cannot be compiled to IR type"
            );
        }
    }
}
