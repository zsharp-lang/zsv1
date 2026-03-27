namespace ZSharp.Compiler
{
    public interface IProxy
        : ICOProxy
    {
        IResult<CompilerObject, Error> ICOProxy.Apply(Func<CompilerObject, IResult<CompilerObject, Error>> fn)
            => Apply(fn);

        public IResult<R, Error> Apply<R>(Func<CompilerObject, IResult<R, Error>> fn) where R : class;
    }
}
