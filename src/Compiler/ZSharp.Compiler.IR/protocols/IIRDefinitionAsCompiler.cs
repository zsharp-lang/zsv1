namespace ZSharp.Compiler
{
    public interface IIRDefinitionAsCompiler
    {
        public IResult<T, Error> CompileDefinition<T>(CompilerObject @object, object? target)
            where T : LIR.Definition;
    }
}
