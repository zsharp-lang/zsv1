namespace ZSharp.HIR
{
    /// <summary>
    /// A definition whose value exists while code runs, such as a local. Code that is
    /// built by code that reads one gets the value it had when that code ran.
    /// </summary>
    public interface Variable
        : Definition
    {
    }
}
