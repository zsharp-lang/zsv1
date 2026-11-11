namespace Package.DotNet
{
    /// <summary>
    /// <c>Ref</c> or <c>Out</c>. Indexing it with a type gives the <see cref="RefType"/> of
    /// that type, so it looks like a generic type, but it isn't one: a reference isn't a
    /// type the CLR instantiates.
    /// </summary>
    public sealed class RefFamily(bool isOut)
        : ZSharp.HIR.Instance
    {
        public bool IsOut { get; set; } = isOut;
    }
}
