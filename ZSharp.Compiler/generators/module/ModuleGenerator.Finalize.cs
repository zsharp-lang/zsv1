﻿using ZSharp.CGObjects;

namespace ZSharp.Compiler
{
    internal sealed partial class ModuleGenerator
    {
        //private readonly HashSet<IR.Module> importedModules = [];

        private void FinalizeCompilation()
        {
            //foreach (var function in Object.IR!.Functions)
            //    LookupImports(function);

            //foreach (var importedMember in Object.ImportedMembers)
            //    importedMember.OnImport(Object);

            IRGenerator.RT.ImportIR(Object.IR!);
        }

        private void LookupImports(IR.Function function)
        {
            // TODO: also check for types

            //foreach (var instruction in function.Body.Instructions)
            //    if (instruction is IR.VM.IHasOperand<IR.IRObject> hasIRObject)
            //        if (
            //            hasIRObject.Operand.Module is not null &&
            //            hasIRObject.Operand.Module != Object.IR &&
            //            !importedModules.Contains(hasIRObject.Operand.Module))
            //            importedModules.Add(hasIRObject.Operand.Module);
        }
    }
}
