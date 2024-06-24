using ZSharp.RAST;

namespace ZSharp.CTRuntime
{
    internal class FunctionBodyCompiler(ZSCompiler compiler)
        : ContextCompilerBase<RFunction, IR.Function>(compiler)
        , IRTDefinitionCompiler
        , IRTStatementCompiler
    {
        private readonly RTCodeCompiler codeCompiler = new(compiler, new RTProcessor(compiler));

        public IRTBinding Compile(RDefinition definition)
        {
            throw new NotImplementedException();
        }

        public IRTBinding Compile(RStatement statement)
        {
            return statement switch
            {
                RReturn @return => Compile(@return),
                _ => throw new NotImplementedException()
            };
        }

        protected override void Compile()
        {
            if (Node!.Body is null) return;

            using (codeCompiler.UseDefinitionCompiler(this))
            using (codeCompiler.UseStatementCompiler(this))
            using (RT.UseScope(Node.Body))
            {
                var body = codeCompiler.Compile(Node.Body);

                IR!.Body.Instructions.AddRange(body.Instructions);
                IR.Body.StackSize = body.StackSize;
            }
        }

        private IRTBinding Compile(RReturn @return)
        {
            var code = @return.Value is null ? Code.Empty() : codeCompiler.Bind(@return.Value).Read(Compiler);

            code.Instructions.Add(new IR.VM.Return());
            code.Type = VM.TypeSystem.Void;

            return new CodeBinding(code);
        }
    }
}
