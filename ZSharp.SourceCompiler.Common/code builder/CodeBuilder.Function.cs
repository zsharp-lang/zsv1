namespace ZSharp.SourceCompiler
{
    partial class CodeBuilder
    {
        private class Function
            : CompilerObject
        {
            internal List<CompilerObject> Body { get; } = new();
            internal List<Local> Locals { get; } = new();
		}
    }
}
