using CommonZ.Utils;

namespace ZSharp.Platform.Runtime.Loaders
{
    partial class EmitLoader
    {
        private delegate T CodeFunctionType<T>();
        private delegate void VoidCodeFunction();

        public Delegate LoadCode(Collection<IR.VM.Instruction> code, IR.IType irReturnType)
        {
            if (code.Count == 0) return () => { };

            if (code.Last() is not IR.VM.Return)
                code.Add(new IR.VM.Return());

            var ilReturnType = Runtime.ImportType(irReturnType);
            var method = new Emit.DynamicMethod(string.Empty, ilReturnType, null, StandaloneModule);

            var codeLoader = new CodeCompiler(new UnboundCodeContext(Runtime, method.GetILGenerator()));
            codeLoader.CompileCode(code);

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
