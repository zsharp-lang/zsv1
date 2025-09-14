namespace ZSharp.Compiler
{
    public interface IIRDefinitionAsCompiler
    {
        public Result<T> CompileDefinition<T>(CompilerObject @object, TargetPlatform? target)
            where T : IRDefinition;
    }
}
