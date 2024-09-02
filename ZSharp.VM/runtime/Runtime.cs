using ZSharp.IR;

namespace ZSharp.VM
{
    public sealed partial class Runtime(RuntimeModule runtimeModule)
    {
        public RuntimeModule RuntimeModule { get; } = runtimeModule;

        public void Initialize()
        {
            var typeObject = new ZSValue(null!);
            typeObject.Type = typeObject;

            irMap[RuntimeModule.TypeSystem.Type] = typeObject;

            irMap[RuntimeModule.TypeSystem.String] = new ZSValue(typeObject);
            irMap[RuntimeModule.TypeSystem.Void] = new ZSValue(typeObject);
        }
    }
}
