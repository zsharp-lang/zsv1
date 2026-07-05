namespace ZSharp.SourceCompiler
{
    public sealed class ImportSystem
    {
        public CompilerObject? ImportFunction { get; set; }

        public static void InstallRTLoader()
        {
            var loadObject = Core.Runtime.ModuleScope.RTLoader.LoadObject;

            Core.Runtime.ModuleScope.RTLoader.LoadObject = @object =>
            {
                var result = loadObject(@object);

                if (result.IsError && @object is COImportResult importResult)
                    result = Result<CompilerObject>.Ok(importResult.CO);

                return result;
            };
        }
    }
}
