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
            var ir = new ModuleGenerator(IRGenerator, module).Run();

            Object.IR!.Submodules.Add(ir);

            return ir;
        }
    }
}
