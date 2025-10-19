using ZSharp.Compiler;

namespace ZSharp.Interpreter
{
    partial class Interpreter
    {
        public IResult<object?, Error> Evaluate(
            CompilerObject @object,
            Platform.Runtime.IEvaluationContext? evaluationContext = null
        )
        {
            if (
                Compiler.IR.CompileCode(@object, Runtime)
                .When(out var irCode)
                .Error(out var error)
            )
                return Result<object?>.Ok(@object);

            var type = irCode!.IsVoid ? RuntimeModule.TypeSystem.Void : irCode.RequireValueType();

            var result = Runtime.Evaluate(irCode.Instructions, type, evaluationContext);

            if (irCode.IsVoid)
                return Result<object?>
                    .Ok(new Objects.RawIRCode(new()));

            if (result is null)
                return Result<object?>
                    .Ok(@object);
            // TODO: should actually be an error!
            //return Result<CompilerObject, string> 
            //    .Error("Non-void expression evaluated to nothing");

            return Result<object?>.Ok(result);
        }
    }
}
