namespace ZSharp.Compiler
{
    /// <summary>
    /// The capability of an error that says an entrypoint doesn't support an operation, as
    /// opposed to supporting it and failing at it. A dispatcher falls through only on it.
    /// </summary>
    public interface Unsupported;

    /// <summary>
    /// Reported by an entrypoint when it does not support an operation.
    /// </summary>
    public sealed class UnsupportedError(string message)
        : ErrorMessage(message)
        , Unsupported
    {
    }
}
