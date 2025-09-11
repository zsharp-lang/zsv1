using Mono.Cecil;

namespace ZSharp.NETCompiler
{
    public sealed class Compiler
    {
        public ModuleDefinition Compile(IR.Module module)
        {
            var result = ModuleDefinition.CreateModule(
                module.Name ?? "<UnnamedModule>",
                new ModuleParameters()
                {
                    Kind = module.HasEntryPoint ? ModuleKind.Console : ModuleKind.Dll
                }
            );

            return result;
        }
    }
}
