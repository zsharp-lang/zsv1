namespace ZSharp.HIR.Code
{
    /// <summary>
    /// Code that is already in the form of the code that builds it. Reifying the code
    /// around it places this as it is, so it runs where that code is built and its
    /// result is what goes here. It can't be loaded, since it only means something inside
    /// code that is reified.
    /// </summary>
    public sealed class Unquote(Expression code)
        : Expression
    {
        public Expression Code { get; set; } = code;
    }

    /// <inheritdoc cref="Unquote"/>
    public sealed class UnquoteStatement(Expression code)
        : Statement
    {
        public Expression Code { get; set; } = code;
    }
}
