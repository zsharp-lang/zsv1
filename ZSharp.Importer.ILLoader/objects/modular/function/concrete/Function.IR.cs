namespace ZSharp.Importer.ILLoader.Objects
{
    partial class Function
    {
        private ZSharp.IR.Function? IR { get; set; }

        private ZSharp.IR.Function GetIR()
        {
            if (IR is null)
            {
                var returnTypeCO = loader.LoadType(IL.ReturnType);
                if (loader.RequireIR().CompileType(returnTypeCO).When(out var returnTypeIR).Error(out var error))
                    throw new InvalidOperationException($"Failed to load return type for method {IL.Name}: {error}");
                IR = new(returnTypeIR!);
                loader.RequireRuntime().AddFunction(IR, IL);
            }

            return IR;
        }
    }
}
