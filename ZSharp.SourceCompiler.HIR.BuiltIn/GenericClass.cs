namespace ZSharp.SourceCompiler.HIR.BuiltIn
{
    public sealed class GenericClass
        : ClassBase
    {
        public List<GenericParameter> GenericParameters { get; init; } = [];
    }
}
