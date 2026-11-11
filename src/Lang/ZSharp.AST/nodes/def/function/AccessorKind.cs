namespace ZSharp.AST
{
    /// <summary>
    /// What an accessor does with its property: <c>get</c> reads it, and <c>set</c> writes it.
    /// </summary>
    public enum AccessorKind
    {
        Get,
        Set,
    }
}
