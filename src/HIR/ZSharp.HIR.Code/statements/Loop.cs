namespace ZSharp.HIR.Code
{
    /// <summary>
    /// A loop that a <see cref="Break"/> or <see cref="Continue"/> can name.
    /// </summary>
    public interface Loop
    {
    }

    /// <summary>
    /// Runs its body while its condition holds. The else part runs when the condition
    /// stops holding, but not when a <see cref="Break"/> leaves the loop.
    /// </summary>
    public sealed class While
        : Statement
        , Loop
    {
        public Expression Condition { get; set; } = null!;

        public Statement Body { get; set; } = null!;

        public Statement? Else { get; set; }
    }

    /// <summary>
    /// A loop that has a value: that of the <see cref="Break"/> that leaves it, or that of
    /// its else part when its condition stops holding.
    /// </summary>
    public sealed class WhileExpression
        : Expression
        , Loop
    {
        public Expression Condition { get; set; } = null!;

        public Statement Body { get; set; } = null!;

        public Expression Else { get; set; } = null!;
    }

    /// <summary>
    /// Leaves a loop, with its value if it is a loop that has one.
    /// </summary>
    public sealed class Break(Loop loop)
        : Statement
    {
        public Loop Loop { get; set; } = loop;

        public Expression? Value { get; set; }
    }

    /// <summary>
    /// Starts the next iteration of a loop.
    /// </summary>
    public sealed class Continue(Loop loop)
        : Statement
    {
        public Loop Loop { get; set; } = loop;
    }
}
