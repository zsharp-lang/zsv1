namespace ZSharp.Compiler
{
    partial class Dispatcher
        : IIRReferenceCompiler
    {
        public IResult<T, Error> CompileReference<T>(CompilerObject @object) where T : class
            => Result<T>.Error(
                $"Object {@object} does not support compile IR reference"
            );
    }
}
