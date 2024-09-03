using ZSharp.CGObjects;
using ZSharp.CGRuntime;

namespace ZSharp.Compiler
{
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
            rtRuntime = new(runtimeModule);
            CG = new(this);
            runtime = new(CG, this, this);
        }

        public CGObject CreateInjector(CGRuntime.HLVM.Injector injector)
            => new RawCode(new(injector()));

        public IR.Module Run(CGObjects.Module module)
        {
            //CompileObject(module);

            //dependencyGraph.Add(module);
            //dependencyGraph.CalculateOrder();

            return new DocumentGenerator(this, module).Run();

            //Run();

            //return module.IR ?? throw new Exception("Compilation failed!");
        }

        private void Run()
        {
            //foreach (var objects in dependencyGraph)
            //{
            //    foreach (var @object in objects)
            //        while ((int)@object.State > (int)dependencyGraph.GetState(@object.Dependency))
            //            if (@object.Dependency is CGObject cgObject)
            //                CompileWithClosure(cgObject);
            //            else
            //                CompileWithClosure(
            //                    CurrentScope.Cache(
            //                        (@object.Dependency as string)!
            //                    ) ?? throw new Exception("Unknown object!")
            //                );

            //    dependencyGraph.CalculateOrder();
            //}
        }
    }
}
