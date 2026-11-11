namespace ZSharp.AST
{
    /// <summary>
    /// An accessor of a property: <c>get Name(): T { ... }</c> reads it, and
    /// <c>set Name(value: T) { ... }</c> writes it. It is a function of its own, named by
    /// the property, which the meta-compiler of the definition it is in pairs with the
    /// property's other accessor.
    /// </summary>
    public sealed class Accessor(FunctionTokens tokens) : Function(tokens)
    {
        public required AccessorKind Kind { get; init; }

        public override string ToString()
            => $"{(Kind == AccessorKind.Set ? "set" : "get")} {Name}({Signature}){(ReturnType is null ? string.Empty : $": {ReturnType}")}" +
            $"{(Body is null ? ";" : " {}")}";
    }
}
