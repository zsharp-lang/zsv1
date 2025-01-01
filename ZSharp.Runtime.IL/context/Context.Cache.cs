namespace ZSharp.Runtime.NET
{
    partial class Context
    {
        private readonly ContextCacheIL2IR toIR = new();

        private readonly ContextCacheIR2IL toIL = new();
    }
}
