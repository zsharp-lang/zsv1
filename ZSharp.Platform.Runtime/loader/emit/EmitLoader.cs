namespace ZSharp.Platform.Runtime.Loaders
{
    internal sealed partial class EmitLoader
    {
        public Runtime Runtime { get; }

        public EmitLoader(Runtime runtime)
        {
            Runtime = runtime;

            StandaloneAssembly = Emit.AssemblyBuilder.DefineDynamicAssembly(
                new IL.AssemblyName("<StandaloneCodeAssembly>"),
                Emit.AssemblyBuilderAccess.RunAndCollect
            );
            StandaloneModule = StandaloneAssembly.DefineDynamicModule(
                "<StandaloneCodeModule>"
            );
        }
    }
}
