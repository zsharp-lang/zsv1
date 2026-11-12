namespace ZSharp.Lang
{
    /// <summary>
    /// What <c>@operator</c> says of a definition: the operator it is. That is one of the
    /// language's operations, as <see cref="Operator.Index"/>, or the functions a piece of
    /// syntax stands for, which an operator literal gives as an instance. It is an
    /// annotation, for what holds the definition to understand, as a class understands its
    /// index operator, and it is encoded with the definition's HIR.
    /// </summary>
    public sealed class OperatorAnnotation(object @operator)
        : HIR.Encoded
    {
        public object Operator { get; set; } = @operator;
    }
}
