namespace ZSharp.Compiler
{
    internal interface IClass
    {
        public CompilerObject Meta { get; }

        public string Name { get; }
    }
}
