using System.Diagnostics.CodeAnalysis;

namespace ZSharp.Compiler.Features.Callable
{
    public interface IArgumentStream
    {
        public bool HasArguments { get; }

        public CompilerObject? PopArgument();

        public CompilerObject? PopArgument(string name);

        public bool PopArgument([NotNullWhen(true)] out CompilerObject? argument)
            => (argument = PopArgument()) is not null;

        public bool PopArgument(string name, [NotNullWhen(true)] out CompilerObject? argument)
            => (argument = PopArgument(name)) is not null;
    }
}
