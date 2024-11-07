using ZSharp.CGObjects;
using ZSharp.RAST;

namespace ZSharp.Compiler
{
    internal sealed partial class ClassSpecCompiler
        : ContextCompiler<ROOPDefinition, ClassSpec>
    {
        public ClassSpecCompiler(Compiler compiler)
            : base(compiler)
        {
            methodBodyCompiler = new(compiler);

            objectBuilder = new(compiler.Context, this, this);
        }

        protected override void Compile()
        {
            if (Node.Bases is not null)
                Result.Bases = [.. Node.Bases.Select(Compiler.CompileNode)];

            if (Node.Content is not null)
            {
                Result.Content = [];
                foreach (var item in Node.Content.Statements)
                    Compiler.CompileNode(item);
            }

            objectBuilder.Build();
            objectBuilder.Clear();
        }

        protected override ClassSpec Create()
            => throw new NotImplementedException();
    }
}
