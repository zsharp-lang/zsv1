namespace ZSharp.Compiler.Features.Callable
{
    public sealed class BoundParameter
    {
        public required CompilerObject ParameterObject { get; init; }

        public required CompilerObject ArgumentObject { get; init; }
    }
}
