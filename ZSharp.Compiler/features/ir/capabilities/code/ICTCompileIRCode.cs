namespace ZSharp.Compiler
{
    public interface ICTCompileIRCode
        : CompilerObject
    {
        public Result<IRCode, Error> CompileIRCode(Compiler compiler);
    }
}
