namespace ZSharp.HIR
{
    /// <summary>
    /// A capability of a kind of definition: definitions of it overload by their name, so
    /// a name bound to one, in a scope, and bound to another, stands for both, which a call
    /// chooses between by its arguments.
    /// </summary>
    public interface Overloadable
    {
    }
}
