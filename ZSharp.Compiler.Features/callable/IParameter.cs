namespace ZSharp.Compiler.Features.Callable
{
    public interface IParameter
    {
        public Result Match(Compiler compiler, IArgumentStream arguments);
    }
}
