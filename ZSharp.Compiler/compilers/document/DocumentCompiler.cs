using ZSharp.CGObjects;
using ZSharp.RAST;

namespace ZSharp.Compiler
{
    internal sealed partial class DocumentCompiler(Compiler compiler)
        : ContextCompiler<RModule, Document>(compiler)
    {
        protected override void Compile()
        {
            if (Node.Content is not null)
                foreach (var statement in Node.Content.Statements)
                    Compile(statement); // TODO: execute
        }

        protected override Document Create()
            => new();

        protected internal override CGObject? Compile(RDefinition definition)
            => definition switch
            {
                RModule module => Compile(module),
                _ => throw new NotImplementedException(),
            };
    }
}
