namespace ZSharp.Compiler
{
    public interface ICompileIRDefinitionIn<Owner>
        where Owner : ZSharp.IR.IRDefinition
    {
        public void CompileIRDefinition(Compiler compiler, Owner owner, TargetPlatform? target);
    }
}
