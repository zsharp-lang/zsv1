namespace ZSharp.Importer.ILLoader.Objects
{
    partial class Method
    {
        private IR.Method? IR { get; set; }

        private IR.Method GetIR(object? target)
        {
            if (IR is null)
            {
                var returnTypeCO = Loader.LoadType(IL.ReturnType);
                if (Loader.IR.CompileType(returnTypeCO, target).When(out var returnTypeIR).Error(out var error))
                    throw new InvalidOperationException($"Failed to load return type for method {IL.Name}: {error}");
                IR = new(returnTypeIR!);
                Loader.RequireRuntime().AddFunction(IR.UnderlyingFunction, IL);
            }

            return IR;
        }
    }
}
