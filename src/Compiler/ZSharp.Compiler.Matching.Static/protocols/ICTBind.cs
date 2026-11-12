namespace ZSharp.Compiler
{
    /// <summary>
    /// A parameter, as matching binds it for a call: the value it takes from the call's
    /// arguments.
    /// </summary>
    public interface ICTBind
    {
        public IResult Bind(ArgumentStream arguments);
    }
}
