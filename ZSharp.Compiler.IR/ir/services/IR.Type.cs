namespace ZSharp.Compiler
{
    partial class IR
    {
        public Result<IType> CompileType(CompilerObject @object)
        {
            if (@object.Is<ICompileIRType>(out var compile))
                return compile.CompileIRType(this);

            return Result<IType>.Error(
                $"Cannot compile type for object of type {@object}"
            );
        }

        public Result<T> CompileType<T>(CompilerObject @object)
            where T : class, IType
        {
            throw new NotImplementedException();
        }
    }
}
