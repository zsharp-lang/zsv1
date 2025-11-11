using IType = ZSharp.IR.IType;

namespace ZSharp.Compiler
{
    public interface ICompileIRType
    {
        public IResult<IType, Error> CompileIRType(Compiler compiler, object? target);
    }

    public interface ICompileIRType<out T>
        : ICompileIRType
        where T : class, IType
    {
        IResult<IType, Error> ICompileIRType.CompileIRType(Compiler compiler, object? target)
            => CompileIRType(compiler, target).When(type => type as IType);

        public new IResult<T, Error> CompileIRType(Compiler compiler, object? target);
    }
}