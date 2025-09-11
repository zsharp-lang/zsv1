using ZSharp.Compiler;

namespace ZSharp.Objects
{
    public class ArgumentMismatchException(CompilerObject callable, Argument[] arguments, Exception? innerException = null)
        : CompilerObjectException(callable, innerException: innerException)
    {
        public CompilerObject Callable => Object;

        public Argument[] Arguments { get; } = arguments;
    }
}
