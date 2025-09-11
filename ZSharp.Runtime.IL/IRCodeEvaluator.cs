using ZSharp.Compiler;
using ZSharp.Objects;

namespace ZSharp.Runtime.NET
{
    public sealed class IRCodeEvaluator(Runtime runtime) 
    {
        private readonly Runtime runtime = runtime;

        public CompilerObject? EvaluateCT(IRCode code, Compiler.Compiler compiler)
        {
            IL.Emit.DynamicMethod method = new(
                string.Empty, 
                runtime.irLoader.LoadType(
                    compiler.IR.CompileType(
                        code.RequireValueType(compiler.TypeSystem.Void)
                    ).Unwrap()
                ), 
                null
            );

            var context = new IR2IL.Code.UnboundCodeContext(runtime.irLoader, method.GetILGenerator());

            new IR2IL.Code.CodeCompiler(context).CompileCode([
                .. code.Instructions,
                new IR.VM.Return()
            ]);

            var result = method.Invoke(null, null);

            if (method.ReturnType == typeof(void)) return null;

            if (result is CompilerObject co)
                return co;

            if (result is ICompileTime coObject)
                return coObject.GetCO();

            return null;
        }
    }
}
