using ZSharp.CGObjects;
using ZSharp.RAST;

namespace ZSharp.Compiler
{
    public sealed class Compiler(IR.RuntimeModule runtimeModule)
    {
        private readonly CGGenerator cg = new();
        private readonly IRGenerator ir = new(runtimeModule);

        public VM.Runtime RT => ir.RT;

        public void Initialize()
        {
            Expose("string", new RawType(ir.RuntimeModule.TypeSystem.String));
            Expose("void", new RawType(ir.RuntimeModule.TypeSystem.Void));
            Expose("type", new RawType(ir.RuntimeModule.TypeSystem.Type));
        }

        public Module CompileCG(RStatement[] statements, string? moduleName = null)
            => cg.Compile(statements, moduleName);

        public IR.Module CompileIR(Module module)
            => ir.Run(module);

        public IR.Module CompileIR(RStatement[] statements, string? moduleName = null)
            => CompileIR(CompileCG(statements, moduleName));

        public void Expose(string name, CGObject @object)
            => ir.Runtime.Expose(name, @object);
    }
}
