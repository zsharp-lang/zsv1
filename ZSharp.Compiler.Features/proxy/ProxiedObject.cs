namespace ZSharp.Compiler.Features
{
    public sealed class ProxiedObject<T>
        where T : class
    {
        public required CompilerObject Origin { get; init; }

        public required T Capability { get; init; }
    }
}
