namespace ZSharp.Compiler
{
    public interface ICompileIRDefinitionAs<out T>
        where T : LIR.Definition
    {
        public IResult<T, Error> CompileIRDefinition(object? target);
    }
}
