namespace ZSharp.Compiler
{
    /// <summary>
    /// Reported by an entrypoint when it does not support an operation, as opposed
    /// to supporting the operation and failing at it.
    /// </summary>
    public sealed class UnsupportedError(string message)
        : ErrorMessage(message)
    {
    }
}
