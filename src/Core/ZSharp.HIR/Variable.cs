namespace ZSharp.HIR
{
    /// <summary>
    /// What has a value only while code runs, such as a local, a parameter, or the object
    /// a method runs on. Code that is built by code that reads one gets the value it had
    /// when that code ran. Being a variable isn't being a definition: a local and a
    /// parameter are both, and the object a method runs on is only a variable.
    /// </summary>
    public interface Variable
    {
    }
}
