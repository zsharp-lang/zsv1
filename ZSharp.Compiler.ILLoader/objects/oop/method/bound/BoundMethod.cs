namespace ZSharp.Compiler.ILLoader.Objects
{
    public sealed partial class BoundMethod
        : CompilerObject
    {
        public required CompilerObject Method { get; set; }

        public required CompilerObject? Object { get; set; }
    }
}
