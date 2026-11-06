namespace Package.DotNet
{
    /// <summary>
    /// Initializes a type itself. It takes nothing, and nothing can call it.
    /// </summary>
    public sealed class StaticConstructor
        : ZSharp.HIR.Definition
    {
        public ZSharp.HIR.Code.Code Body { get; } = new();
    }
}
