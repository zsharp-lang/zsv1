namespace ZSharp.LIR.CLR
{
    /// <summary>
    /// Who can reach a definition of the CLR's, as the CLR has it. A top-level type is
    /// <see cref="Public"/> or else internal to its assembly.
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
}
