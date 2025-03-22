namespace ZSharp.Parser
{
    public class ParseError(string? message = null) : Exception(message)
    {
    }
}
