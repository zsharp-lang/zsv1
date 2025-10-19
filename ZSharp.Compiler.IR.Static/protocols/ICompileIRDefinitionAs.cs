namespace ZSharp.Compiler
{
    public interface ICompileIRDefinitionAs<T>
        where T : ZSharp.IR.IRDefinition
    {
        public IResult<T, Error> CompileIRDefinition(Compiler compiler, TargetPlatform? target);
    }
}
