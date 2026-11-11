namespace ZSharp.Compiler
{
    /// <summary>
    /// Members an object has from outside its own, as extension methods or the methods of
    /// a trait it implements: whoever provides them decides which objects have them.
    /// </summary>
    public interface IExternalMembers
    {
        /// <returns>
        /// The members with the name that the object has from this provider, each bound to
        /// the object, or none.
        /// </returns>
        public IEnumerable<CompilerObject> Members(CompilerObject @object, MemberName name);
    }
}
