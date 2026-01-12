using CommonZ.Utils;

using Debuggable = System.Diagnostics.DebuggableAttribute;

namespace ZSharp.Platform.Runtime.Loaders
{
    partial class EmitLoader
    {
        private delegate T CodeFunctionType<T>();
        private delegate void VoidCodeFunction();

        public Delegate LoadCode(
            Collection<IR.VM.Instruction> code, 
            IR.IType irReturnType, 
            IEvaluationContext evaluationContext
        )
        {
            if (code.Count == 0) return () => { };

            if (code.Last() is not IR.VM.Return)
                code.Add(new IR.VM.Return());

            var ilReturnType = Runtime.ImportType(irReturnType);
            var ilGenerator = evaluationContext.DefineCode(ilReturnType);

            var codeLoader = new CodeCompiler(new UnboundCodeContext(Runtime, ilGenerator));
            codeLoader.CompileCode(code);

            var method = evaluationContext.LoadMethod();

            return
                ilReturnType != typeof(void)
                ? method.CreateDelegate(
                    typeof(CodeFunctionType<>)
                    .MakeGenericType(ilReturnType)
                    )
                : method.CreateDelegate(
                    typeof(VoidCodeFunction)
                    )
                ;
        }
    }
}
