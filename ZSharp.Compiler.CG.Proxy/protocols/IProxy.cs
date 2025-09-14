namespace ZSharp.Compiler
{
    public interface IProxy
    {
        public R Apply<R>(Func<CompilerObject, R> fn);
    }
}
