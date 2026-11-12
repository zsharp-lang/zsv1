namespace ZSharp.LIR
{
    /// <summary>
    /// The capability of a statement that a <see cref="Break"/> inside it can leave: the
    /// code goes on after it. A loop has it, and so can any statement that is left early.
    /// </summary>
    public interface Breakable;
}
