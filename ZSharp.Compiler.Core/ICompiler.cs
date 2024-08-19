namespace ZSharp.Compiler
{
    public interface ICompiler
    {
        public CGRuntime.ICodeGenerator CG { get; }

        public IRCode Read(CTObject @object);
    }
}
