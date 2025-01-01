namespace ZSharp.Compiler.IRLoader
{
    public sealed partial class IRLoader : Feature
    {
        public IRLoader(Compiler compiler) : base(compiler)
        {
            Initialize();
        }

        private void Initialize()
        {
            foreach (var type in new CompilerObject[] {
                Compiler.TypeSystem.Type,
                Compiler.TypeSystem.Float32,
                Compiler.TypeSystem.String,
                Compiler.TypeSystem.Int32,
                Compiler.TypeSystem.Void,
                Compiler.TypeSystem.Boolean,
            })
                Context.Types.Cache(Compiler.CompileIRType(type), type);
        }
    }
}
