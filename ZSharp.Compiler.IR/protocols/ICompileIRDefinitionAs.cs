namespace ZSharp.Compiler
{
    public interface ICompileIRDefinitionAs<T>
        where T : IRDefinition
    {
        public Result<T> CompileIRDefinition(IR ir, TargetPlatform? target);
    }
}
