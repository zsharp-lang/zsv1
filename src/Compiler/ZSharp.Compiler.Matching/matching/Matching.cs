namespace ZSharp.Compiler
{
    /// <summary>
    /// Matching something against what it is meant to fit: a value against a pattern, and
    /// a call's arguments against a parameter. Neither is part of the general operations
    /// of code: a CO can use matching when it is called or set, but doesn't have to.
    /// </summary>
    public partial class Matching
    {
        public Matching Clone() => (Matching)MemberwiseClone();
    }
}
