using ZSharp.CGObjects;
using ZSharp.RAST;

namespace ZSharp.Compiler
{
    internal sealed partial class ClassSpecCompiler
    {
        //private void Define(Class @class, ROOPDefinition node)
        //    => classBodyCompiler.Compile(node, @class); // TODO: support metaclasses

        private void Define(Field field, RLetDefinition node)
        {
            //if (global.Initializer is not null)
            //{
            //    var initializerCode = Compiler.CompileIRCode(global.Initializer);

            //    var initIR = Result.InitFunction.IR!;
            //    initIR.Body.StackSize = Math.Max(initIR.Body.StackSize, initializerCode.MaxStackSize);
            //    initIR.Body.Instructions.AddRange([
            //        ..initializerCode.Instructions,
            //        new IR.VM.SetGlobal(global.IR!)
            //        ]);
            //}
        }

        //private void Define(Global global, RVarDefinition node)
        //{
        //    if (global.Initializer is null && node.Value is not null)
        //        global.Initializer = Compiler.CompileNode(node.Value);

        //    // TODO: initialize to default value
        //    if (global.Initializer is not null)
        //    {
        //        var initializerCode = Compiler.CompileIRCode(global.Initializer);

        //        var initIR = Result.InitFunction.IR!;
        //        initIR.Body.StackSize = Math.Max(initIR.Body.StackSize, initializerCode.MaxStackSize);
        //        initIR.Body.Instructions.AddRange([
        //            ..initializerCode.Instructions,
        //            new IR.VM.SetGlobal(global.IR!)
        //            ]);
        //    }
        //}

        private void Define(Method method, RFunction node)
            => methodBodyCompiler.Compile(node, method);
    }
}
