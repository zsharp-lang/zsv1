namespace ZSharp.Lang
{
    /// <summary>
    /// A definition marked to be exported, which is what <c>@export</c> returns. Whatever
    /// registers the result of a definition's decorators records it, if it knows how: a
    /// document exports the definition under the alias, or its name if it has none, and
    /// binds its name to the definition itself.
    /// </summary>
    public sealed class Export(HIR.Instance definition, string? alias)
    {
        public HIR.Instance Definition { get; } = definition;

        public string? Alias { get; } = alias;
    }
}
