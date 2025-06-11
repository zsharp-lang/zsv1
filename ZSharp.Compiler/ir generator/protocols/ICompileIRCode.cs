namespace ZSharp.Compiler
{
    public interface ICompileIRCode : ICTCompileIRCode
    {
        Result<IRCode, Error> ICTCompileIRCode.CompileIRCode(Compiler compiler)
            => Result<IRCode, Error>.Ok(CompileIRCode(compiler));

        public new IRCode CompileIRCode(Compiler compiler);
    }
}
