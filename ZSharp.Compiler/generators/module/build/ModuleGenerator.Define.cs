using ZSharp.CGObjects;

namespace ZSharp.Compiler
{
    internal sealed partial class ModuleGenerator
    {
        private void Define(RTFunction function)
            => new FunctionBodyGenerator(IRGenerator, function).Run();

        private void Define(Global global)
        {
            if (global.Initializer is not null && global.IR!.Initializer is null)
            {
                var initializerCode = IRGenerator.Read(IRGenerator.CG.Run(global.Initializer));

                var initIR = Object.InitFunction.IR!;
                initIR.Body.StackSize = Math.Max(initIR.Body.StackSize, initializerCode.MaxStackSize);
                initIR.Body.Instructions.AddRange([
                    ..initializerCode.Instructions,
                    new IR.VM.SetGlobal(global.IR)
                    ]);
            }
        }
    }
}
