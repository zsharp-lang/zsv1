namespace ZSharp.Importer.ILLoader.Objects
{
    public sealed partial class Interface
        : CompilerObject
    {
        public string Name => IL.Name;

        public Interface(Type @interface, ILLoader loader)
        {
            IL = @interface;
            Loader = loader;

            IR = loader.Compiler.IR.CompileDefinition<IR.Interface>(this, loader.Runtime).Unwrap();
        }
    }
}
