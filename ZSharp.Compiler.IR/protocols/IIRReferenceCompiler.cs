namespace ZSharp.Compiler
{
    public interface IIRReferenceCompiler
    {
        public IResult<T, Error> CompileReference<T>(CompilerObject @object, object? target)
            where T : class;
    }
}
