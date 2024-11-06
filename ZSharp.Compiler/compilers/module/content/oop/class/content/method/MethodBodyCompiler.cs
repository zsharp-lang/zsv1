using ZSharp.CGObjects;
using ZSharp.RAST;

namespace ZSharp.Compiler
{
    internal sealed partial class MethodBodyCompiler(Compiler compiler)
        : ContextCompiler<RFunction, Method>(compiler)
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

        protected override Method Create()
        {
            throw new NotImplementedException();
        }
    }
}
