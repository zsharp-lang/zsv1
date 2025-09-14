using IType = ZSharp.IR.IType;

namespace ZSharp.Compiler
{
    public interface ICompileIRType
    {
        public Result<IType> CompileIRType(Compiler compiler);
    }

    public interface ICompileIRType<T>
        : ICompileIRType
        where T : class, IType
    {
        Result<IType> ICompileIRType.CompileIRType(Compiler compiler)
            => CompileIRType(compiler).When(type => type as IType);

        public new Result<T> CompileIRType(Compiler compiler);
    }
}