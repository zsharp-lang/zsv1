﻿using ZSharp.CGObjects;
using ZSharp.RAST;

namespace ZSharp.Compiler
{
    internal sealed partial class ModuleCompiler
    {
        private void Define(ModuleOOPObject oop, ROOPDefinition node)
        {
            classBodyCompiler.Compile(node, oop);

            if (Compiler.CompileIRObject(oop.Object!) is IR.OOPType ir)
                Result.IR!.Types.Add(ir);
        }

        private void Define(Global global, RLetDefinition node)
        {
            if (global.Initializer is not null)
            {
                var initializerCode = Compiler.CompileIRCode(global.Initializer);

                var initIR = Result.InitFunction.IR!;
                initIR.Body.StackSize = Math.Max(initIR.Body.StackSize, initializerCode.MaxStackSize);
                initIR.Body.Instructions.AddRange([
                    ..initializerCode.Instructions,
                    new IR.VM.SetGlobal(global.IR!)
                    ]);
            }
        }

        private void Define(Global global, RVarDefinition node)
        {
            if (global.Initializer is null && node.Value is not null)
                global.Initializer = Compiler.CompileNode(node.Value);

            // TODO: initialize to default value
            if (global.Initializer is not null)
            {
                var initializerCode = Compiler.CompileIRCode(global.Initializer);

                var initIR = Result.InitFunction.IR!;
                initIR.Body.StackSize = Math.Max(initIR.Body.StackSize, initializerCode.MaxStackSize);
                initIR.Body.Instructions.AddRange([
                    ..initializerCode.Instructions,
                    new IR.VM.SetGlobal(global.IR!)
                    ]);
            }
        }

        private void Define(RTFunction function, RFunction node)
            => functionBodyCompiler.Compile(node, function);
    }
}
