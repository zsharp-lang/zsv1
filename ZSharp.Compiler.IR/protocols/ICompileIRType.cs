namespace ZSharp.Compiler
{
    public interface ICompileIRType
    {
        public Result<IType> CompileIRType(IR ir);
    }

    public interface ICompileIRType<T>
        : ICompileIRType
        where T : class, IType
    {
        Result<IType> ICompileIRType.CompileIRType(IR ir)
            => CompileIRType(ir).When(type => type as IType);

        public new Result<T> CompileIRType(IR ir);
    }
}