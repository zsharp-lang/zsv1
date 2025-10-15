namespace ZSharp.Platform.Runtime
{
    internal sealed class ExecutionEvaluationContextFactory
        : IEvaluationContextFactory
    {
        public Emit.AssemblyBuilder StandaloneAssembly { get; }

        public Emit.ModuleBuilder StandaloneModule { get; }

        public ExecutionEvaluationContextFactory()
        {
            StandaloneAssembly = Emit.AssemblyBuilder.DefineDynamicAssembly(
                new("<ExecutionEvaluationContextModule>"),
                Emit.AssemblyBuilderAccess.RunAndCollect
            );
            StandaloneModule = StandaloneAssembly.DefineDynamicModule(
                "<ExecutionEvaluationContextModule>"
            );
        }

        IEvaluationContext IEvaluationContextFactory.CreateEvaluationContext()
            => new ExecutionEvaluationContext(StandaloneModule);
    }
}
