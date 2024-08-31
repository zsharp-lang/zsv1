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

            IRGenerator.Runtime.Context.Enter(Object);

            foreach (var (name, item) in IRGenerator.CurrentScope)
                switch (item)
                {
                    case FunctionOverloadGroup group: Object.Members[name] = group; break;
                    case Global global: Object.Members[name] = global; break;
                    case Module module: Object.Members[name] = module; break;
                    case RTFunction function: Object.Members[name] = function; break;
                    default: break;
                }

            IRGenerator.Runtime.Context.Leave();

            return Object.IR;
        }
    }
}
