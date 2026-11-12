namespace ZSharp.Compiler
{
    /// <summary>
    /// What is referred to as a function bound to an object, as a method reached through an
    /// object is: the function takes the object first, and a delegate of it is closed over
    /// the object, which is null for what is bound to nothing.
    /// </summary>
    public interface ICompileIRReceiver
    {
        public CompilerObject? Receiver { get; }
    }
}
