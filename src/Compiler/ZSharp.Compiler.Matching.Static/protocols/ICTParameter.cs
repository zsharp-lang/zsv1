namespace ZSharp.Compiler
{
    /// <summary>
    /// A parameter, as matching gives its value for a call from the call's arguments.
    /// </summary>
    public interface ICTParameter
    {
        public IResult Match(ArgumentStream arguments);
    }
}
