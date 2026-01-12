namespace ZSharp.Platform.Runtime
{
    internal sealed class DebuggableEvaluationContextFactory
        : IEvaluationContextFactory
    {
        private const string AssemblyFormat = "ZSharpPersistedAssembly_{0}";
        private int persistedAssemblyCount = 0;

        public string? OutputPath { get; init; }

        IEvaluationContext IEvaluationContextFactory.CreateEvaluationContext()
            => new DebuggableEvaluationContext(string.Format(AssemblyFormat, persistedAssemblyCount++))
            {
                OutputPath = OutputPath
            };
    }
}
