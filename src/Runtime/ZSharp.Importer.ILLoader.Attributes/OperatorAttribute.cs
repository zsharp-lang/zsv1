namespace ZSharp.Importer.ILLoader
{
    /// <summary>
    /// Marks a method as a function a piece of syntax stands for: an operator, as in
    /// <c>[Operator("+")]</c>, or <c>import</c>. Several methods for one piece of syntax
    /// are its overloads.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public sealed class OperatorAttribute(string @operator) : Attribute
    {
        public string Operator { get; } = @operator;
    }
}
