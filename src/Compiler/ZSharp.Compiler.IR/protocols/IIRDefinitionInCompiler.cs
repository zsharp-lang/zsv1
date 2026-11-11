namespace ZSharp.Compiler
{
    public interface IIRDefinitionInCompiler
    {
        public Error? CompileDefinition<Owner>(CompilerObject @object, Owner owner, object? target)
            where Owner : LIR.Definition;
    }
}
