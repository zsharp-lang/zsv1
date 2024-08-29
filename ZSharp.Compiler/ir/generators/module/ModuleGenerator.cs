using ZSharp.CGObjects;

namespace ZSharp.Compiler
{
    internal sealed partial class ModuleGenerator(IRGenerator irGenerator, Module module)
        : IRGeneratorBase<Module, IR.Module>(irGenerator, module)
    {
        public override void CompileObject(CGObject @object)
            => Compile(@object);

        protected override IR.Module Compile()
        {
            Object.IR = new(Object.Name);
            Object.InitFunction.IR = new(IRGenerator.RuntimeModule.TypeSystem.Void);

            foreach (var _ in IRGenerator.Runtime.Run(Object.Content))
                throw new Exception("Module content should not return any values.");

            Build();

            return Object.IR;
        }
    }
}
