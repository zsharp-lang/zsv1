namespace ZSharp.Compiler
{
    public interface ICOProxy
    {
        public IResult Apply(Func<CompilerObject, IResult> fn);
    }
}
