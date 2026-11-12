namespace ZSharp.Compiler
{
    /// <summary>
    /// A pattern that knows how a value matches it, as a name pattern binds what it
    /// matched: the condition, as a CO.
    /// </summary>
    public interface ICTPattern
    {
        public IResult Match(CompilerObject value);
    }
}
