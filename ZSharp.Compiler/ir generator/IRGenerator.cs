using ZSharp.CGObjects;
using ZSharp.CGRuntime;

namespace ZSharp.Compiler
{
    internal partial class IRGenerator
        : ICodeInjector
        , ICompiler
    {
        private readonly CodeGenerator codeGenerator;

        internal IR.RuntimeModule RuntimeModule { get; }

        ICodeGenerator ICompiler.Generate => codeGenerator;

        public CodeGenerator Generate => codeGenerator;

        public IRGenerator(IR.RuntimeModule runtimeModule)
        {
            RuntimeModule = runtimeModule;
            codeGenerator = new(this);
            RT = new(runtimeModule);
            CG = new(codeGenerator, this, this);
        }

        // TODO: remove injector when all objects are converted to CGObjects
        // so we no longer need to inject instructions
        public CGObject CreateInjector(CGRuntime.HLVM.Injector injector)
            => new RawCode(new(injector()));

        public IR.Module RunAsDocument(Module module)
            => new DocumentGenerator(this, module).Run();

        public IR.Module RunAsModule(Module module)
            => new ModuleGenerator(this, module).Run();
    }
}
