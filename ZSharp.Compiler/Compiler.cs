using ZSharp.RAST;

namespace ZSharp.Compiler
{
    public sealed class Compiler(IR.RuntimeModule runtimeModule)
    {
        private readonly CGGenerator cg = new();
        private readonly IRGenerator ir = new(runtimeModule);

        public CGObjects.Module CompileCG(RStatement[] statements, string? moduleName = null)
            => cg.Compile(statements, moduleName);

        public IR.Module CompileIR(CGObjects.Module module)
            => ir.Run(module);

        public IR.Module CompileIR(RStatement[] statements, string? moduleName = null)
            => CompileIR(CompileCG(statements, moduleName));

        public void Expose(string name, CGObject @object)
            => ir.Runtime.Expose(name, @object);
    }
}
