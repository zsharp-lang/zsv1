namespace ZSharp.Compiler
{
    public interface ICompileIRDefinitionAs<T>
        where T : ZSharp.IR.IRDefinition
    {
        public Result<T> CompileIRDefinition(Compiler compiler, TargetPlatform? target);
    }
}
