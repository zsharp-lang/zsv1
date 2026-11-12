namespace ZSharp.Platform.Runtime
{
    /// <summary>
    /// Loads LIR modules into the running process.
    /// </summary>
    public sealed partial class Runtime
        : ClrPlatform
    {
        private readonly Emit.ModuleBuilder module;

        private int nextId;

        public Runtime()
        {
            var assembly = Emit.AssemblyBuilder.DefineDynamicAssembly(
                new IL.AssemblyName("ZSharp.Runtime"),
                Emit.AssemblyBuilderAccess.Run
            );

            module = assembly.DefineDynamicModule("ZSharp.Runtime");
        }

        private string Unique(string? name)
            => $"{name ?? "<Anonymous>"}${nextId++}";
    }
}
