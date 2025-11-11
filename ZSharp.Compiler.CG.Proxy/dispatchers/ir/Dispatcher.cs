namespace ZSharp.Compiler.IRDispatchers.Proxy
{
    public partial class Dispatcher(Compiler compiler)
    {
        private readonly Compiler compiler = compiler;
        private IR @base;

        private ref IR IR => ref compiler.IR;

        public void Apply()
        {
            @base = compiler.IR;

            ref var ir = ref IR;

            ir.CompileCode = CompileCode;
            ir.DefinitionInCompiler = this;
            ir.DefinitionAsCompiler = this;
            ir.ReferenceCompiler = this;
            ir.CompileType = CompileType;
            ir.TypeCompiler = this;
        }
    }
}
