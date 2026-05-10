namespace ZSharp.SourceCompiler.Objects
{
    partial class Definition
        : ITyped
    {
        private readonly CompilerObject type;

        CompilerObject ITyped.Type => type;

        public CompilerObject Type
        {
            init => type = value;
        }
    }
}
