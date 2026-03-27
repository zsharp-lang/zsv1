namespace ZSharp.SourceCompiler.Objects
{
    internal sealed partial class Constructor
        : CompilerObject
        , IConstructor
    {
        public CompilerObject? Body { get; set; }
    }
}
