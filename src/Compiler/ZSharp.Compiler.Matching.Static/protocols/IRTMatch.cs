namespace ZSharp.Compiler
{
    /// <summary>
    /// What describes values at runtime, as their type does, and knows whether one of them
    /// matches a pattern: the condition, as a CO.
    /// </summary>
    public interface IRTMatch
    {
        public IResult Match(CompilerObject value, CompilerObject pattern);
    }
}
