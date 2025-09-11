namespace ZSharp.SourceCompiler
{
    internal sealed partial class BooleanLiteral(bool value)
        : CompilerObject
    {
        private readonly bool value = value;
    }
}
