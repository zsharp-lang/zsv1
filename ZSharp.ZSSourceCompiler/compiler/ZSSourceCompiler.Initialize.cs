namespace ZSharp.ZSSourceCompiler
{
    public partial class ZSSourceCompiler
    {
        private void Initialize()
        {
            InitializeTypeSystem();

            InitializeImportSystem();
        }

        private void InitializeTypeSystem()
        {
            Context.GlobalScope.Set("bool", Compiler.TypeSystem.Boolean);
            Context.GlobalScope.Set("string", Compiler.TypeSystem.String);
            Context.GlobalScope.Set("type", Compiler.TypeSystem.Type);
            Context.GlobalScope.Set("void", Compiler.TypeSystem.Void);

            Context.GlobalScope.Set("i32", Compiler.TypeSystem.Int32);

            Context.GlobalScope.Set("f32", Compiler.TypeSystem.Float32);
        }

        private void InitializeImportSystem()
        {
            ImportSystem.ImportFunction = StringImporter;

            StringImporter.Importers.Add("std", StandardLibraryImporter);
            StringImporter.Importers.Add("zs", ZSImporter);
        }
    }
}
