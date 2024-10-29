using ZSharp.CGObjects;
using ZSharp.RAST;

namespace ZSharp.Compiler
{
    internal sealed partial class ModuleCompiler
        : ContextCompiler<RModule, Module>
    {
        public ModuleCompiler(Compiler compiler)
            : base(compiler)
        {
            functionBodyCompiler = new(compiler);
            classBodyCompiler = new(compiler);

            objectBuilder = new(compiler.Context, this, this);
        }

        protected override void Compile()
        {
            Result.IR = new(Node.Name);

            Result.InitFunction.IR = new(Compiler.RuntimeModule.TypeSystem.Void);

            if (Node.Content is not null)
                foreach (var statement in Node.Content.Statements)
                    Compiler.CompileNode(statement);

            objectBuilder.Build();
            objectBuilder.Clear();

            if (Result.InitFunction.IR.HasBody)
                Result.IR.Functions.Insert(0, Result.InitFunction.IR);

            Compiler.Runtime.ImportIR(Result.IR);
        }

        protected override Module Create()
            => new(Node.Name ?? throw new());
    }
}
