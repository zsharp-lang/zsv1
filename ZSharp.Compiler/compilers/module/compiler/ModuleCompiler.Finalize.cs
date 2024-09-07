namespace ZSharp.Compiler
{
    internal sealed partial class ModuleCompiler
    {
        private void FinalizeCompilation()
        {
            Compiler.Runtime.ImportIR(Result.IR!);
        }
    }
}
