namespace ZSharp.Importer.IRLoader
{
    public sealed partial class IRLoader : Feature
    {
        public IRLoader(Compiler compiler) : base(compiler)
        {
            Initialize();
        }

        private void Initialize()
        {
            foreach (var type in new IType[] {
                Compiler.TypeSystem.Type,
                Compiler.TypeSystem.Float32,
                Compiler.TypeSystem.String,
                Compiler.TypeSystem.Int32,
                Compiler.TypeSystem.Void,
                Compiler.TypeSystem.Boolean,
                Compiler.TypeSystem.Object,
            })
                Context.Types.Cache(Compiler.CompileIRType(type), type);

            Context.Objects.Cache(Compiler.TypeSystem.ArrayType.IR, Compiler.TypeSystem.ArrayType);
            Context.Objects.Cache(Compiler.TypeSystem.ReferenceType.IR, Compiler.TypeSystem.ReferenceType);
        }
    }
}
