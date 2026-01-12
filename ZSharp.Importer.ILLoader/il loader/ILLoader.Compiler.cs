namespace ZSharp.Importer.ILLoader
{
    partial class ILLoader
    {
        public Compiler.Compiler Compiler { get; }

        public ref Compiler.IR IR => ref Compiler.IR;
    }
}
