namespace ZSharp.Compiler
{
    public interface ICompileIRType
    {
        public IRType CompileIRType(Compiler compiler);
    }

    public interface ICompileIRType<T> : ICompileIRType
        where T : IRType
    {
        IRType ICompileIRType.CompileIRType(Compiler compiler)
            => CompileIRType(compiler);

        public new T CompileIRType(Compiler compiler);
    }
}
