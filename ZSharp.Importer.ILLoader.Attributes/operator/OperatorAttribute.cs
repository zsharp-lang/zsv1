namespace ZSharp.Importer.ILLoader
{
    [AttributeUsage(
        AttributeTargets.Method,
        AllowMultiple = false
    )]
    public sealed class OperatorAttribute(string @operator) : Attribute
    {
        public string Operator { get; } = @operator;

        public required OperatorKind Kind { get; init; }
    }
}
