using ZSharp.CGObjects;

namespace ZSharp.Compiler
{
    internal sealed partial class FunctionBodyGenerator(IRGenerator irGenerator, RTFunction function)
        : IRGeneratorBase<RTFunction, IR.Function>(irGenerator, function)
    {
        public override void CompileObject(CGObject @object)
        {
            switch (@object)
            {
                case Local local: Compile(local); break;
                default: break;
            }
        }

        protected override IR.Function Compile()
        {
            using (IRGenerator.ContextOf(Object))
                if (Object.HasBody)
                {
                    Code body = new();

                    foreach (var item in IRGenerator.CG.Run(Object.Body))
                        body.Append(IRGenerator.Read(item));

                    Object.IR!.Body.Instructions.AddRange(body.Instructions);

                    foreach (var instruction in body.Instructions)
                        if (instruction is IR.VM.IHasOperand<IR.ModuleMember> hasIR)
                            if (hasIR.Operand.Module is not null && hasIR.Operand.Module != Object.IR.Module)
                                AddReference(hasIR.Operand);

                    Object.IR.Body.StackSize = body.MaxStackSize;
                }

            if (Object.ReturnType is null)
                Object.IR!.ReturnType = InferReturnType();

            return Object.IR!;
        }

        private IRType InferReturnType()
        {
            throw new NotImplementedException();
        }

        private void AddReference(IR.ModuleMember member)
        {
            var moduleImports = Object.IR!.Module!.ImportedModules.FirstOrDefault(
                imports => imports.Imported == member.Module
            );

            var module = member as IR.Module ?? member.Module;

            if (moduleImports is null)
                Object.IR.Module.ImportedModules.Add(moduleImports = new(member.Module!));

            moduleImports.ImportedMembers.Add(new()
            {
                Member = member,
            });
        }
    }
}
