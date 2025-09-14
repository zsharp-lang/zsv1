namespace ZSharp.Compiler
{
    public interface IIRDefinitionInCompiler
    {
        public bool CompileDefinition<Owner>(CompilerObject @object, Owner owner, TargetPlatform? target)
            where Owner : IRDefinition;
    }
}
