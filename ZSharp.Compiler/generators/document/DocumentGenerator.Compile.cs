using ZSharp.CGObjects;

namespace ZSharp.Compiler
{
    internal sealed partial class DocumentGenerator
    {
        private IR.IRObject Compile(CGObject @object)
            => @object switch
            {
                Module module => Compile(module),
                _ => throw new NotImplementedException(),
            };

        private IR.Module Compile(Module module)
        {
            if (module.Name is not null && module.Name != string.Empty)
                IRGenerator.CurrentScope.Cache(module.Name, module);

            var ir = new ModuleGenerator(IRGenerator, module).Run();

            Object.IR!.Submodules.Add(ir);

            return ir;
        }
    }
}
