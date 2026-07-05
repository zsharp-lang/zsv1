namespace ZSharp.Compiler.IRDispatchers.Static
{
    public partial class Dispatcher(Compiler compiler)
    {
        private readonly Compiler compiler = compiler;
        private IR @base;

        public void Apply()
        {
            @base = compiler.IR.Clone();

            var ir = compiler.IR;

            ir.CompileCode = CompileCode;
            ir.DefinitionInCompiler = this;
            ir.DefinitionAsCompiler = this;
            ir.ReferenceCompiler = this;
            ir.CompileType = CompileType;
            ir.TypeCompiler = this;
        }
    }
}
