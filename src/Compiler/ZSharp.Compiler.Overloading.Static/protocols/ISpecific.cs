namespace ZSharp.Compiler
{
    /// <summary>
    /// A candidate that knows whether it is at least as specific as another one.
    /// </summary>
    public interface ISpecific
    {
        public bool IsAsSpecificAs(CompilerObject other);
    }
}
