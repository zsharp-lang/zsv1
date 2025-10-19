namespace ZSharp.Compiler
{
    partial class Dispatcher
        : IIRTypeCompiler
    {
        public IResult<IType, Error> CompileType(CompilerObject @object)
            => Result<IType>.Error(
                $"Object {@object} does not support compile IR type"
            );

        IResult<T, Error> IIRTypeCompiler.CompileType<T>(CompilerObject @object)
            => Result<T>.Error(
                $"Object {@object} does not support compile IR type"
            );
    }
}
