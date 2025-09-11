namespace ZSharp.Compiler
{
    public interface ICompileIRCode
    {
        public Result<IRCode> CompileIRCode(IR ir, TargetPlatform? target);
    }
}
