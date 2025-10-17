namespace ZSharp.Compiler
{
    public interface IProxy
    {
        public Result<R> Apply<R>(Func<CompilerObject, Result<R>> fn) where R : class;
    }
}
