namespace Package.DotNet
{
    /// <summary>
    /// Who can reach a definition of the CLR's, as the CLR has it. A top-level type is
    /// <see cref="Public"/> or <see cref="Internal"/> only.
    /// </summary>
    public enum Accessibility
    {
        Public,
        Private,
        Internal,
        Protected,
        ProtectedInternal,
        PrivateProtected,
    }

    /// <summary>
    /// The capability of a definition of the CLR's that has an accessibility. A property
    /// has none of its own: its accessors have it. A node made without one says public,
    /// as one already built was before accessibility was written; what compiles a
    /// definition gives it the default of where it is.
    /// </summary>
    public interface Accessible
    {
        public Accessibility Accessibility { get; set; }
    }
}
