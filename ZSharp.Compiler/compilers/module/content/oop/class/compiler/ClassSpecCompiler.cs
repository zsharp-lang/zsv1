using ZSharp.RAST;

namespace ZSharp.Compiler
{
    internal sealed partial class ClassSpecCompiler
        : ContextCompiler<ROOPDefinition, ModuleOOPObject>
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
                Result.Spec.Bases = [.. Node.Bases.Select(Compiler.CompileNode)];

            if (Node.Content is not null)
            {
                Result.Spec.Content = [];
                foreach (var item in Node.Content.Statements)
                    Compiler.CompileNode(item);
            }

            objectBuilder.Build();
            objectBuilder.Clear();
        }

        protected override ModuleOOPObject Create()
            => throw new Exception();
    }
}
