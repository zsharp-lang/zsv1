using System.Reflection;
using System.Reflection.Emit;

namespace ZSharp.Platform.Runtime
{
    internal sealed class ExecutionEvaluationContext(ModuleBuilder moduleBuilder)
        : IEvaluationContext
    {
        private readonly ModuleBuilder moduleBuilder = moduleBuilder;
        private DynamicMethod? dynamicMethod;

        ModuleBuilder IEvaluationContext.Module => throw new NotImplementedException();

        ILGenerator IEvaluationContext.DefineCode(Type returnType)
        {
            if (dynamicMethod is not null)
                throw new InvalidOperationException("Code has already been defined.");

            return (dynamicMethod = new(
                "<Code>",
                returnType,
                [],
                moduleBuilder
            )).GetILGenerator();
        }

        MethodInfo IEvaluationContext.LoadMethod()
            => dynamicMethod ?? throw new InvalidOperationException("Code has not been defined.");
    }
}
