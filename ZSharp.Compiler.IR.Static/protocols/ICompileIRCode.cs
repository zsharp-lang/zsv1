namespace ZSharp.Compiler
{
    public interface ICompileIRCode
    {
        public Result<IRCode> CompileIRCode(Compiler compiler, TargetPlatform? target);
    }
}
