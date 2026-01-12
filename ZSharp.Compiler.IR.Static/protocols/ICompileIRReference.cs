namespace ZSharp.Compiler
{
    public interface ICompileIRReference<out T>
        where T : class
    {
        public IResult<T, Error> CompileIRReference(Compiler compiler);
    }
}
