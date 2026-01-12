namespace ZSharp.Compiler
{
    public interface IIRDefinitionAsCompiler
    {
        public IResult<T, Error> CompileDefinition<T>(CompilerObject @object, TargetPlatform? target)
            where T : IRDefinition;
    }
}
