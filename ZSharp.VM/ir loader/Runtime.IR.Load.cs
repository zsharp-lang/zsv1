using ZSharp.IR;

namespace ZSharp.VM
{
    public sealed partial class Runtime
    {
        public ZSFunction LoadIR(Function function)
            => LoadIR(function as IRObject) as ZSFunction ?? throw new();

        public ZSModule LoadIR(Module module)
        {
            var zsModule = (ZSModule)LoadIR(module as IRObject);

            if (module.Functions.Count > 0 && module.Functions[0].Name is null)
            {
                var initFunction = LoadIR(module.Functions[0]);

                Run(new(initFunction.Code, initFunction.StackSize), new ZSObject[initFunction.LocalCount]);
            }

            return zsModule;
        }
    }
}
