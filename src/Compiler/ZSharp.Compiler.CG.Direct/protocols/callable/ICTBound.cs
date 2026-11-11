namespace ZSharp.Compiler
{
    /// <summary>
    /// A function bound to an object, as a method reached through an object is: the function
    /// takes the object first, and calling it gives the rest. What makes a delegate of it
    /// closes the delegate over the object. Bound to nothing, as a static method is, it has
    /// none.
    /// </summary>
    public interface ICTBound
    {
        public CompilerObject? Receiver { get; }
    }
}
