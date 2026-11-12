namespace Package.DotNet
{
    /// <summary>
    /// Metadata attached to an entity, as a constructor and its arguments, the way the
    /// CLR describes custom attributes.
    /// </summary>
    public sealed class CustomMetadata(ZSharp.HIR.Instance constructor)
    {
        public ZSharp.HIR.Instance Constructor { get; set; } = constructor;

        public List<ZSharp.HIR.Instance> Arguments { get; } = [];
    }
}
