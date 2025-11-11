namespace ZSharp.SourceCompiler.Objects
{
    internal sealed partial class Function
        : CompilerObject
        , IFunction
    {
        public CompilerObject? Body { get; set; }
    }
}
