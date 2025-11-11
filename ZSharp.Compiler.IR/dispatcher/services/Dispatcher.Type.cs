namespace ZSharp.Compiler
{
    partial class Dispatcher
        : IIRTypeCompiler
    {
        public IResult<IType, Error> CompileType(CompilerObject @object, object? target)
            => Result<IType>.Error(
                $"Object {@object} does not support compile IR type"
            );

        IResult<T, Error> IIRTypeCompiler.CompileType<T>(CompilerObject @object, object? target)
            => Result<T>.Error(
                $"Object {@object} does not support compile IR type"
            );
    }
}
