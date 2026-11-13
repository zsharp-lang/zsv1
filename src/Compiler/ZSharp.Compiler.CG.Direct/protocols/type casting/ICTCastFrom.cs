namespace ZSharp.Compiler
{
    /// <summary>
    /// A type that knows how a value is cast to it, as a cast is written: what a value that
    /// doesn't know how to go to it is asked after.
    /// </summary>
    public interface ICTCastFrom
    {
        public IResult Cast(CompilerObject value);
    }
}
