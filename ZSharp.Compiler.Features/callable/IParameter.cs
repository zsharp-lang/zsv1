namespace ZSharp.Compiler.Features.Callable
{
    public interface IParameter
    {
        public IResult Match(Compiler compiler, IArgumentStream arguments);
    }
}
