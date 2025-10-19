namespace ZSharp.Compiler
{
    public interface ICompileIRCode
    {
        public IResult<IRCode, Error> CompileIRCode(Compiler compiler, TargetPlatform? target);
    }
}
