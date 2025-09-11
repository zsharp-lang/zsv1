namespace ZSharp.SourceCompiler
{
    internal sealed partial class StringLiteral(string value)
        : CompilerObject
    {
        private readonly string value = value;
    }
}
