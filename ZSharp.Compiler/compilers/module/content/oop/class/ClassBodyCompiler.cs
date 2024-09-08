using ZSharp.CGObjects;
using ZSharp.RAST;

namespace ZSharp.Compiler
{
    internal sealed partial class ClassBodyCompiler(Compiler compiler)
        : ContextCompiler<ROOPDefinition, Class>(compiler)
    {
        protected override void Compile()
        {
            Result.IR = new(Node.Name);

            if (Node.Content is not null)
                foreach (var item in Node.Content.Statements)
                    Compiler.CompileNode(item);
        }

        protected override Class Create()
            => throw new NotImplementedException();
    }
}
