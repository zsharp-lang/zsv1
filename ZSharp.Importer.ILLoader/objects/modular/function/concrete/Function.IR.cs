namespace ZSharp.Importer.ILLoader.Objects
{
    partial class Function
    {
        private IR.Function? IR { get; set; }

        private IR.Function GetIR(object? target)
        {
            if (IR is null)
            {
                var returnTypeCO = Loader.LoadType(IL.ReturnType);
                if (Loader.IR.CompileType(returnTypeCO, target).When(out var returnTypeIR).Error(out var error))
                    throw new InvalidOperationException($"Failed to load return type for method {IL.Name}: {error}");
                IR = new(returnTypeIR!);
                Loader.RequireRuntime().AddFunction(IR, IL);
            }

            return IR;
        }
    }
}
