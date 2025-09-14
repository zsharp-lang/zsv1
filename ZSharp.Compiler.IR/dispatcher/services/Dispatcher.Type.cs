namespace ZSharp.Compiler
{
    partial class Dispatcher
        : IIRTypeCompiler
    {
        public Result<IType> CompileType(CompilerObject @object)
            => Result<IType>.Error(
                $"Object {@object} does not support compile IR type"
            );

        Result<T> IIRTypeCompiler.CompileType<T>(CompilerObject @object)
            => Result<T>.Error(
                $"Object {@object} does not support compile IR type"
            );
    }
}
