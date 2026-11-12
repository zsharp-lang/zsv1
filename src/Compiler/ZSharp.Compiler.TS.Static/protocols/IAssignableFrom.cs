namespace ZSharp.Compiler
{
    /// <summary>
    /// A type that says which types can be assigned to it.
    /// </summary>
    public interface IAssignableFrom
    {
        public bool IsAssignableFrom(CompilerObject source);
    }
}
