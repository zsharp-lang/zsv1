using ZSharp.CGObjects;

namespace ZSharp.Compiler
{
    internal sealed partial class DocumentGenerator(IRGenerator irGenerator, Module module)
        : IRGeneratorBase<Module, IR.Module>(irGenerator, module)
    {
        protected override IR.Module Compile()
        {
            Object.IR = new(Object.Name);

            foreach (var _ in IRGenerator.Runtime.Run(Object.Content))
                throw new Exception("Document level CG code must not result with CG objects!");

            return Object.IR;
        }

        public override void CompileObject(CGObject @object)
            => Compile(@object);
    }
}
