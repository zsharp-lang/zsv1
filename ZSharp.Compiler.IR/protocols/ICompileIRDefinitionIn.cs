namespace ZSharp.Compiler
{
    public interface ICompileIRDefinitionIn<Owner>
        where Owner : IRDefinition
    {
        public void CompileIRDefinition(IR ir, Owner owner, TargetPlatform? target);
    }
}
