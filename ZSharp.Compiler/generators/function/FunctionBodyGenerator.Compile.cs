using ZSharp.CGObjects;

namespace ZSharp.Compiler
{
    internal sealed partial class FunctionBodyGenerator
    {
        private void Compile(Local local)
        {
            if (IRGenerator.CurrentScope.Contains(local.Name))
                IRGenerator.CurrentScope.Uncache(local.Name);

            IRGenerator.CurrentScope.Cache(local.Name, local);

            if (local.Initializer is null)
                throw new NotImplementedException();

            var initializerCode = IRGenerator.Read(IRGenerator.CG.Run(local.Initializer));

            var type = local.Type is null
                ? initializerCode.RequireValueType()
                : IRGenerator.EvaluateType(local.Type);

            local.IR = new(local.Name, type);

            Object.IR!.Body.Locals.Add(local.IR);

            Object.IR.Body.Instructions.AddRange([
                ..initializerCode.Instructions,
                new IR.VM.SetLocal(local.IR)
            ]);

            Object.IR.Body.StackSize = Math.Max(Object.IR.Body.StackSize, initializerCode.MaxStackSize);
        }
    }
}
