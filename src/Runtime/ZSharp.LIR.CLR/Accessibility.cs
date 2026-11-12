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

    /// <summary>
    /// A CLR platform's mark on a definition that says who can reach it: a class, a
    /// member of one, or a module's function or global. LIR has no accessibility, so a
    /// target that reads none makes the definition public, which still works.
    /// </summary>
    public sealed class Accessible(Accessibility accessibility)
        : LIR.Metadata
    {
        public Accessibility Accessibility { get; } = accessibility;

        /// <summary>
        /// The accessibility a definition's metadata says, or public if it says none.
        /// </summary>
        public static Accessibility Of(IEnumerable<LIR.Metadata> metadata)
            => metadata.OfType<Accessible>().LastOrDefault()?.Accessibility ?? Accessibility.Public;
    }
}
