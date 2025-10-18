namespace ZSharp.Importer.ILLoader.Objects
{
    partial class Method
    {
        private readonly Signature signature = new();

        public CompilerObject Signature => signature;

        public CompilerObject ReturnType { get; }
    }
}
