namespace ZSharp.Compiler
{
    partial class IR
    {
        public Result<T> CompileReference<T>(CompilerObject @object)
            where T : class
        {
            if (@object.Is<ICompileIRReference<T>>(out var compile))
                return compile.CompileIRReference(this);

            return Result<T>.Error(
                $"Cannot compile reference for object of type {@object}"
            );
        }
    }
}
