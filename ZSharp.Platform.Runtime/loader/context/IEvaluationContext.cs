namespace ZSharp.Platform.Runtime
{
    public interface IEvaluationContext
    {
        public Emit.ModuleBuilder Module { get; }

        public Emit.ILGenerator DefineCode(Type returnType);

        public IL.MethodInfo LoadMethod();
    }
}
