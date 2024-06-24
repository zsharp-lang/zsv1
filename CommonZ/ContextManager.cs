namespace CommonZ.Utils
{
    public sealed class ContextManager(Action onExit) : IDisposable
    {
        private readonly Action onExit = onExit;

        public static readonly ContextManager Empty = new(() => { });

        public void Dispose()
        {
            onExit();
        }
    }
}
