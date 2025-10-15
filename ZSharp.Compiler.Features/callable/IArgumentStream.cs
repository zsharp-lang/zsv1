namespace ZSharp.Compiler.Features.Callable
{
    public interface IArgumentStream
    {
        public bool HasArguments { get; }

        public CompilerObject? PopArgument();

        public CompilerObject? PopArgument(string name);
    }
}
