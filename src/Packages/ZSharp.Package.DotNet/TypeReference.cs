namespace Package.DotNet
{
    /// <summary>
    /// A .NET type, usable both in a type slot and as a value.
    /// </summary>
    public interface TypeReference
        : ZSharp.HIR.Type
        , ZSharp.HIR.Instance
    {
    }
}
