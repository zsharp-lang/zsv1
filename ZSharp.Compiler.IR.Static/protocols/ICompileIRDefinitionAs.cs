namespace ZSharp.Compiler
{
    public interface ICompileIRDefinitionAs<out T>
        where T : ZSharp.IR.IRDefinition
    {
        public IResult<T, Error> CompileIRDefinition(Compiler compiler, TargetPlatform? target);
    }
}
