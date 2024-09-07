namespace ZSharp.Compiler
{
    public interface ICompiler
    {
        public Code CompileIRCode(CTObject @object);
    }
}
