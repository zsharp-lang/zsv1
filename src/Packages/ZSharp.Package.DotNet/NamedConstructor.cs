namespace Package.DotNet
{
    /// <summary>
    /// What is said of a constructor with a name, and of what stands for it. The CLR has no
    /// named constructors, so one is a private constructor that takes a private marker type
    /// of its own, named <c>&lt;name&gt;</c>, and a static method of the name that calls it:
    /// both say they are the named constructor. It is kept, so it is there as the code runs.
    /// </summary>
    public sealed class NamedConstructor(string name)
        : ZSharp.HIR.Kept
    {
        public string Name { get; } = name;
    }
}
