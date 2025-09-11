namespace ZSharp.Runtime.Loaders
{
    partial class ModuleLoader
    {
        private void LoadGlobal(IR.Global global)
        {
            var field = Globals.DefineField(
                global.Name,
                Loader.Runtime.ImportType(global.Type),
                IL.FieldAttributes.Public | IL.FieldAttributes.Static
            );

            Loader.Runtime.AddGlobal(global, field);
        }
    }
}
