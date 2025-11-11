namespace ZSharp.Importer.ILLoader.Objects
{
    partial class Constructor
    {
        private IR.Constructor? IR { get; set; }

        private IR.Constructor GetIR(object? target)
        {
            if (IR is null)
            {
                var returnTypeCO = Loader.LoadType(typeof(void));
                if (Loader.IR.CompileType(returnTypeCO, target).When(out var returnTypeIR).Error(out var error))
                    throw new InvalidOperationException($"Failed to load return type for method {IL.Name}: {error}");
                IR = new(string.Empty)
                {
                    Method = new(returnTypeIR!)
                };
                Loader.RequireRuntime().AddFunction(IR.Method.UnderlyingFunction, IL);
            }

            return IR;
        }
    }
}
