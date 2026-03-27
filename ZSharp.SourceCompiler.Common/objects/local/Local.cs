namespace ZSharp.SourceCompiler.Objects
{
    internal sealed partial class Local
        : CompilerObject
        , ILocal
    {
        public CompilerObject? Body { get; set; }
    }
}
