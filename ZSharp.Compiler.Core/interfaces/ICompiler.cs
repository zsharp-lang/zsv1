namespace ZSharp.Compiler
{
    public interface ICompiler
    {
        public CGRuntime.ICodeGenerator CG { get; }

        public Code Read(CTObject @object);
    }
}
