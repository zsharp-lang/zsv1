namespace ZSharp.Compiler
{
    /// <summary>
    /// A type that says which types it can be assigned to.
    /// </summary>
    public interface IAssignableTo
    {
        public bool IsAssignableTo(CompilerObject target);
    }
}
