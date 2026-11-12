namespace ZSharp.Compiler
{
    public interface ICompileIRDefinitionIn<in Owner>
        where Owner : LIR.Definition
    {
        public Error? CompileIRDefinition(Owner owner, object? target);
    }
}
