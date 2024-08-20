using ZSharp.CGRuntime;

namespace ZSharp.Compiler
{
    internal class Code(IRCode code)
        : CGObject
        , ICTReadable
    {
        public CGObject Type => throw new NotImplementedException();

        public IRCode Read(ICompiler compiler)
            => code;
    }

    internal partial class IRGenerator
        : ICodeInjector
        , ICompiler
    {
        private readonly Runtime runtime;

        private readonly Queue<CGObject> objectQueue = [];

        private readonly CodeGenerator CG;

        ICodeGenerator ICompiler.CG => CG;

        internal Runtime Runtime => runtime;

        public IRGenerator()
        {
            CG = new(this);
            runtime = new(CG, this, this);
        }

        public IR.Module Run(CGObjects.Module module)
        {
            var ir = Definition(module);
            Run();
            return ir;
        }

        public void Run()
        {
            while (objectQueue.Count > 0)
                Compile(objectQueue.Dequeue());
        }

        public void Enqueue(CGObject @object)
            => objectQueue.Enqueue(@object);

        public CGObject CreateInjector(CGRuntime.HLVM.Injector injector)
            => new Code([.. injector()]);
    }
}
