using ZSharp.CGRuntime;

namespace ZSharp.Compiler
{
    internal class CodeObject(Code code)
        : CGObject
        , ICTReadable
    {
        public CGObject Type => throw new NotImplementedException();

        public Code Read(ICompiler compiler)
            => code;
    }

    internal partial class IRGenerator
        : ICodeInjector
        , ICompiler
    {
        private readonly Runtime runtime;

        private readonly Queue<CGObject> objectQueue = [];

        private readonly CodeGenerator CG;

        internal IR.RuntimeModule RuntimeModule { get; }

        ICodeGenerator ICompiler.CG => CG;

        internal Runtime Runtime => runtime;

        public IRGenerator(IR.RuntimeModule runtimeModule)
        {
            RuntimeModule = runtimeModule;
            CG = new(this);
            runtime = new(CG, this, this);
        }

        public IR.Module Run(CGObjects.Module module)
        {
            CompileObject(module);

            Run();

            return module.IR ?? throw new Exception("Compilation failed!");
        }

        public void Run()
        {
            foreach (var objects in dependencyGraph)
                foreach (var @object in objects)
                    Compile(@object);
        }

        public CGObject CreateInjector(CGRuntime.HLVM.Injector injector)
            => new CodeObject(new(injector()));
    }
}
