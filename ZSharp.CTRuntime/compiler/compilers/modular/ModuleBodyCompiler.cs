using ZSharp.RAST;

namespace ZSharp.CTRuntime
{
    internal sealed class ModuleBodyCompiler(ZSCompiler compiler)
                : ContextCompilerBase<RModule, IR.Module>(compiler)
        , ICTDefinitionCompiler
    {
        private readonly FunctionCompiler functionCompiler = new(compiler);
        private readonly CTCodeCompiler codeCompiler = new(compiler, new CTProcessor(compiler));

        protected override void Compile()
        {
            if (Node!.Content is null) return;

            using (codeCompiler.UseCompiler(this))
            using (CT.UseScope(Node))
                foreach (var item in Node.Content.Statements)
                    Compile(item);
        }

        private void Compile(RStatement statement) 
            => codeCompiler.Compile(statement);

        public ICTBinding Compile(RDefinition definition)
        {
            return definition switch
            {
                RFunction function => Compile(function),
                RLetDefinition let => Compile(let),
                _ => throw new NotImplementedException(),
            };
        }

        public ICTBinding Compile(RFunction function)
        {
            //throw new NotImplementedException();

            var functionIR = functionCompiler.Compile(function);

            IR!.Functions.Add(functionIR);

            return CT.Scope.Cache(function.Id)!;
        }

        private ICTBinding Compile(RLetDefinition let)
        {
            var global = 
                CT.Scope.Cache<Global>(let.Id) 
                ?? Context.Set(let.Id, new Global(new(let.Name!, null!), null!));

            var value = codeCompiler.Bind(let.Value).Read(Compiler);

            if (let.Type is not null)
                global.Type = codeCompiler.Bind(let.Type).Read(Compiler);
            else
                global.Type = VM.Interpreter.TypeOf(value);

            //global.IR.Type = VM.Interpreter.GetIR<IR.IType>(global.Type);

            IR!.Globals.Add(global!.IR);

            if (Compiler.Interpreter.LoadIR(IR) is not VM.ZSModule moduleObject) 
                throw new("Invalid module object");
            VM.Interpreter.Resize(moduleObject, IR.Globals.Count);

            moduleObject.SetGlobal(global.IR, value);

            return global;
        }
    }
}
