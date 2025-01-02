using ZSharp.Compiler;

namespace ZSharp.Objects
{
    public class ArgumentMismatchException(CompilerObject callable, Argument[] arguments)
        : CompilerObjectException(callable)
    {
        public CompilerObject Callable => Object;

        public Argument[] Arguments { get; } = arguments;
    }
}
