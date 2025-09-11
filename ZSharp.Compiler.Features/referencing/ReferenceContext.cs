using CommonZ.Utils;

namespace ZSharp.Objects
{
    public sealed class ReferenceContext(ReferenceContext? parent = null)
    {
        public Cache<CompilerObject, CompilerObject> CompileTimeValues { get; init; } = new()
        {
            Parent = parent?.CompileTimeValues
        };

        public ReferenceContext? Parent { get; } = parent;

        public CompilerObject? Scope { get; init; } = null;

        public CompilerObject this[CompilerObject key]
        {
            get => CompileTimeValues.Cache(key) ?? throw new KeyNotFoundException(
                $"Key {key} was not found in the reference context{(Scope is null ? string.Empty : $" {Scope}")}."
            );
            set => CompileTimeValues.Cache(key, value);
        }
    }
}
