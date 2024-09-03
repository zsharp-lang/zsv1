namespace ZSharp.Compiler
{
    public interface ICompiler
    {
        public CGRuntime.ICodeGenerator Generate { get; }

        public Code Read(CTObject @object);
    }
}
