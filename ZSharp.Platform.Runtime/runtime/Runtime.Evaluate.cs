using CommonZ.Utils;

namespace ZSharp.Platform.Runtime
{
    partial class Runtime
    {
        public object? Evaluate(
            Collection<IR.VM.Instruction> code, 
            IR.IType type, 
            IEvaluationContext? evaluationContext = null
        )
        {
            return Loader.LoadCode(
                code, type, 
                evaluationContext ?? EvaluationContextFactory.CreateEvaluationContext()
            ).DynamicInvoke(null);
        }
    }
}
