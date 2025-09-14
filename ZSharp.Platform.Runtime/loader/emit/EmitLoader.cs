using Debuggable = System.Diagnostics.DebuggableAttribute;

namespace ZSharp.Platform.Runtime.Loaders
{
    public sealed partial class EmitLoader
    {
        public Runtime Runtime { get; }

        internal EmitLoader(Runtime runtime)
        {
            Runtime = runtime;
            
            StandaloneAssembly = new Emit.PersistedAssemblyBuilder(
                new("<StandaloneCodeModule>"), 
                typeof(void).Assembly,
                [
                    new(
                        typeof(Debuggable).GetConstructor([
                            typeof(Debuggable.DebuggingModes)
                        ]) ?? throw new(),
                        [
                            Debuggable.DebuggingModes.Default
                        ]
                    )
                ]
            );
           
            StandaloneModule = StandaloneAssembly.DefineDynamicModule(
                "<StandaloneCodeModule>"
            );
        }
    }
}
