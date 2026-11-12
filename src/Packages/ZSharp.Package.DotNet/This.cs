namespace Package.DotNet
{
    /// <summary>
    /// The object an instance method or a constructor runs on, a variable of its code,
    /// which a function in the code can capture, as it captures a parameter.
    /// </summary>
    public sealed class This
        : ZSharp.HIR.Variable
        , ZSharp.HIR.Instance
    {
    }
}
