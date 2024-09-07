using ZSharp.CGObjects;
using ZSharp.RAST;

namespace ZSharp.Compiler
{
    internal sealed partial class FunctionBodyCompiler(Compiler compiler)
        : ContextCompiler<RFunction, Function>(compiler)
    {
        protected override void Compile()
        {
            if (Node.Body is not null)
            {
                Code body = Compiler.CompileIRCode(Compiler.CompileNode(Node.Body));

                Result.IR!.Body.Instructions.AddRange(body.Instructions);
                Result.IR.Body.StackSize = Math.Max(Result.IR.Body.StackSize, body.MaxStackSize);
            }
        }

        protected override Function Create()
        {
            throw new NotImplementedException();
        }
    }
}
