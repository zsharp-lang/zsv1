namespace ZSharp.Compiler
{
    public interface IProxy
    {
        public IResult<R, Error> Apply<R>(Func<CompilerObject, IResult<R, Error>> fn) where R : class;
    }
}
