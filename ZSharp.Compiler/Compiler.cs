using ZSharp.CGObjects;
using ZSharp.RAST;

namespace ZSharp.Compiler
{
    public sealed class Compiler(IR.RuntimeModule runtimeModule)
    {
        private readonly IRGenerator ir = new(runtimeModule);

        public VM.Runtime RT => ir.RT;

        public void Initialize()
        {
            Expose("string", new RawType(ir.RuntimeModule.TypeSystem.String));
            Expose("void", new RawType(ir.RuntimeModule.TypeSystem.Void));
            Expose("type", new RawType(ir.RuntimeModule.TypeSystem.Type));
        }

        public Module CompileCG(RStatement[] statements, string? moduleName = null)
            => new CGCompiler.Compiler().Compile(new RModule(moduleName ?? string.Empty)
            {
                Content = new RBlock([..statements])
            });

        public IR.Module CompileIR(Module module)
            => ir.RunAsDocument(module);

        public void Expose(string name, CGObject @object)
            => ir.CG.Expose(name, @object);
    }
}
