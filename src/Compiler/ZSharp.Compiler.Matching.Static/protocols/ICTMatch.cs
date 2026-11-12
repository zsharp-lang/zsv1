namespace ZSharp.Compiler
{
    /// <summary>
    /// A value that knows whether it matches a pattern: the condition, as a CO.
    /// </summary>
    public interface ICTMatch
    {
        public IResult Match(CompilerObject pattern);
    }
}
