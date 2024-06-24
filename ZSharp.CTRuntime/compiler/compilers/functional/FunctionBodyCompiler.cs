using ZSharp.RAST;

namespace ZSharp.CTRuntime
{
    internal class FunctionBodyCompiler(ZSCompiler compiler)
        : ContextCompilerBase<RFunction, IR.Function>(compiler)
        , IRTDefinitionCompiler
    {
        private readonly RTCodeCompiler codeCompiler = new(compiler, new RTProcessor(compiler));

        public IRTBinding Compile(RDefinition definition)
        {
            throw new NotImplementedException();
        }

        protected override void Compile()
        {
            if (Node!.Body is null) return;

            using (codeCompiler.UseCompiler(this))
            using (RT.UseScope(Node.Body))
            {
                var body = codeCompiler.Compile(Node.Body);

                IR!.Body.Instructions.AddRange(body.Instructions);
                IR.Body.StackSize = body.StackSize;
            }
        }
    }
}
