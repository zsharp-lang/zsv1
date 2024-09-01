using ZSharp.CGObjects;

namespace ZSharp.Compiler
{
    internal sealed partial class FunctionBodyGenerator(IRGenerator irGenerator, RTFunction function)
        : IRGeneratorBase<RTFunction, IR.Function>(irGenerator, function)
    {
        public override void CompileObject(CGObject @object)
        {
            throw new NotImplementedException();
        }

        protected override IR.Function Compile()
        {
            using (IRGenerator.ContextOf(Object))
                if (Object.HasBody)
                {
                    Code body = new();

                    foreach (var item in IRGenerator.Runtime.Run(Object.Body))
                        body.Append(IRGenerator.Read(item));

                    Object.IR!.Body.Instructions.AddRange(body.Instructions);

                    Object.IR.Body.StackSize = body.MaxStackSize;
                }

            return Object.IR!;
        }
    }
}
